using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetOrgPostStats;

public sealed class GetOrgPostStatsHandler(
    IMembershipRepository membershipRepository,
    IPostRepository       postRepository)
    : IRequestHandler<GetOrgPostStatsQuery, OrgPostStatsResult>
{
    public async Task<OrgPostStatsResult> Handle(
        GetOrgPostStatsQuery query, CancellationToken cancellationToken)
    {
        _ = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        var startOfMonth = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var foundPosts = await postRepository.CountAsync(new PostFilters
        {
            OrganizationId = query.OrgId,
            PostType       = PostType.Found
        }, cancellationToken);
        var foundThisMonth = await postRepository.CountAsync(new PostFilters
        {
            OrganizationId = query.OrgId,
            PostType       = PostType.Found,
            Time           = new TimeFilter(startOfMonth, null)
        }, cancellationToken);

        return new OrgPostStatsResult
        {
            FoundPosts = foundPosts,
            FoundThisMonth = foundThisMonth
        };
    }
}
