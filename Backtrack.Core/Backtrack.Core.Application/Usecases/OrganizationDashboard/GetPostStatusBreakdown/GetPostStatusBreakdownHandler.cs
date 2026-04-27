using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetPostStatusBreakdown;

public sealed class GetPostStatusBreakdownHandler(
    IMembershipRepository membershipRepository,
    IPostRepository       postRepository)
    : IRequestHandler<GetPostStatusBreakdownQuery, PostStatusBreakdownResult>
{
    private static readonly IReadOnlyList<string> DisplayedStatuses =
    [
        "InStorage", "ReturnScheduled", "Returned", "Archived", "Expired"
    ];

    public async Task<PostStatusBreakdownResult> Handle(
        GetPostStatusBreakdownQuery query, CancellationToken cancellationToken)
    {
        _ = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        var orgBreakdown  = await postRepository.GetStatusBreakdownByOrgAsync(query.OrgId, null,         cancellationToken);
        var mineBreakdown = await postRepository.GetStatusBreakdownByOrgAsync(query.OrgId, query.UserId, cancellationToken);

        return new PostStatusBreakdownResult
        {
            Org  = BuildGroup(orgBreakdown),
            Mine = BuildGroup(mineBreakdown)
        };
    }

    private static StatusBreakdownGroup BuildGroup(
        Dictionary<(PostType Type, string EffectiveStatus), int> breakdown)
    {
        return new StatusBreakdownGroup
        {
            Lost  = BuildPostTypeBreakdown(breakdown, PostType.Lost),
            Found = BuildPostTypeBreakdown(breakdown, PostType.Found)
        };
    }

    private static PostTypeBreakdown BuildPostTypeBreakdown(
        Dictionary<(PostType Type, string EffectiveStatus), int> breakdown,
        PostType postType)
    {
        var total = breakdown
            .Where(kv => kv.Key.Type == postType)
            .Sum(kv => kv.Value);

        var statuses = DisplayedStatuses.Select(status =>
        {
            breakdown.TryGetValue((postType, status), out var count);
            return new StatusCount
            {
                Status = status,
                Count  = count,
                Pct    = total > 0 ? (int)Math.Round(count * 100.0 / total) : 0
            };
        }).ToList();

        return new PostTypeBreakdown { Total = total, Statuses = statuses };
    }
}
