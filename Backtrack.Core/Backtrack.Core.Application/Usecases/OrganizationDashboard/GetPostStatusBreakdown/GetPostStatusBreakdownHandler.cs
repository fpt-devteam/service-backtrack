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
    private static readonly IReadOnlyList<PostStatus> DisplayedStatuses =
    [
        PostStatus.InStorage, PostStatus.Returned, PostStatus.Archived, PostStatus.Expired
    ];

    public async Task<PostStatusBreakdownResult> Handle(
        GetPostStatusBreakdownQuery query, CancellationToken cancellationToken)
    {
        _ = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        var orgBreakdown  = await postRepository.GetStatusBreakdownByOrgAsync(query.OrgId, null,         cancellationToken);
        var mineBreakdown = await postRepository.GetStatusBreakdownByOrgAsync(query.OrgId, query.UserId, cancellationToken);

        var orgTotal = orgBreakdown
            .Where(kv => kv.Key.Type == PostType.Found)
            .Sum(kv => kv.Value);

        return new PostStatusBreakdownResult
        {
            Org  = BuildGroup(orgBreakdown,  orgTotal),
            Mine = BuildGroup(mineBreakdown, orgTotal)
        };
    }

    private static StatusBreakdownGroup BuildGroup(
        Dictionary<(PostType Type, PostStatus Status), int> breakdown,
        int denominator)
    {
        var total = breakdown
            .Where(kv => kv.Key.Type == PostType.Found)
            .Sum(kv => kv.Value);

        var statuses = DisplayedStatuses.Select(status =>
        {
            breakdown.TryGetValue((PostType.Found, status), out var count);
            return new StatusCount
            {
                Status = status.ToString(),
                Count  = count,
                Pct    = denominator > 0 ? (int)Math.Round(count * 100.0 / denominator) : 0
            };
        }).ToList();

        return new StatusBreakdownGroup
        {
            Found = new PostTypeBreakdown { Total = total, Statuses = statuses }
        };
    }
}
