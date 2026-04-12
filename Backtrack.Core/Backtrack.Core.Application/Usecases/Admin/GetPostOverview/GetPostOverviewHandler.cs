using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetPostOverview;

public sealed class GetPostOverviewHandler(
    IUserRepository      userRepository,
    IPostRepository      postRepository,
    IPostMatchRepository postMatchRepository) : IRequestHandler<GetPostOverviewQuery, PostDetailOverviewResult>
{
    public async Task<PostDetailOverviewResult> Handle(GetPostOverviewQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var startOfMonth = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var (breakdown, chartData, matchedCount, newThisMonth) = (
            await postRepository.GetBreakdownAsync(cancellationToken),
            await postRepository.GetGrowthChartAsync(query.Months, cancellationToken),
            await postMatchRepository.CountMatchedPostsAsync(cancellationToken),
            await postRepository.CountAsync(new PostFilters { Time = new TimeFilter(startOfMonth, null) }, cancellationToken)
        );

        int Sum(PostStatus s) => breakdown.Where(kv => kv.Key.Status == s).Sum(kv => kv.Value);

        var total  = breakdown.Values.Sum();
        var active = Sum(PostStatus.Active);

        return new PostDetailOverviewResult
        {
            Total        = total,
            NewThisMonth = newThisMonth,
            LostCount    = breakdown.Where(kv => kv.Key.Type == PostType.Lost).Sum(kv => kv.Value),
            FoundCount   = breakdown.Where(kv => kv.Key.Type == PostType.Found).Sum(kv => kv.Value),
            ByStatus = new PostStatusBreakdown
            {
                Active          = active,
                InStorage       = Sum(PostStatus.InStorage),
                ReturnScheduled = Sum(PostStatus.ReturnScheduled),
                Returned        = Sum(PostStatus.Returned),
                Archived        = Sum(PostStatus.Archived),
                Expired         = Sum(PostStatus.Expired)
            },
            MatchRate = total > 0 ? Math.Round((double)matchedCount / total * 100, 2) : 0,
            Chart     = chartData.Select(c => new PeriodCount(c.Period, c.Count)).ToList()
        };
    }
}
