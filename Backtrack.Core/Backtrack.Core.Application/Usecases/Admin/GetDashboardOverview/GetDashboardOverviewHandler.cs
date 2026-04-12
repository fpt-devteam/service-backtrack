using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Application.Usecases.PostExplorations;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetDashboardOverview;

public sealed class GetDashboardOverviewHandler(
    IUserRepository         userRepository,
    IOrganizationRepository organizationRepository,
    IPostRepository         postRepository,
    IPaymentHistoryRepository paymentHistoryRepository,
    ISubscriptionRepository subscriptionRepository) : IRequestHandler<GetDashboardOverviewQuery, DashboardOverviewResult>
{
    public async Task<DashboardOverviewResult> Handle(GetDashboardOverviewQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var (userCounts, orgCounts, breakdown, revSummary, subCounts, mrr) = (
            await userRepository.GetCountsAsync(cancellationToken),
            await organizationRepository.GetCountsAsync(cancellationToken),
            await postRepository.GetBreakdownAsync(cancellationToken),
            await paymentHistoryRepository.GetRevenueSummaryAsync(cancellationToken),
            await subscriptionRepository.GetActiveCountsAsync(cancellationToken),
            await subscriptionRepository.GetMrrAsync(cancellationToken)
        );

        // Post counts from breakdown
        int GetCount(PostType type, PostStatus status) =>
            breakdown.TryGetValue((type, status), out var c) ? c : 0;

        var startOfMonth = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var newPostsThisMonth = await postRepository.CountAsync(
            new PostFilters { Time = new TimeFilter(startOfMonth, null) }, cancellationToken);

        var totalPosts = breakdown.Values.Sum();
        var lost       = breakdown.Where(kv => kv.Key.Type == PostType.Lost).Sum(kv => kv.Value);
        var found      = breakdown.Where(kv => kv.Key.Type == PostType.Found).Sum(kv => kv.Value);

        return new DashboardOverviewResult
        {
            Users = new UserOverviewStats
            {
                Total        = userCounts.Total,
                Active       = userCounts.Active,
                Inactive     = userCounts.Total - userCounts.Active,
                NewThisMonth = userCounts.NewThisMonth
            },
            Organizations = new OrgOverviewStats
            {
                Total        = orgCounts.Total,
                Active       = orgCounts.Active,
                NewThisMonth = orgCounts.NewThisMonth
            },
            Posts = new PostOverviewStats
            {
                Total          = totalPosts,
                LostCount      = lost,
                FoundCount     = found,
                ActiveCount    = breakdown.Where(kv => kv.Key.Status == PostStatus.Active).Sum(kv => kv.Value),
                InStorageCount = breakdown.Where(kv => kv.Key.Status == PostStatus.InStorage).Sum(kv => kv.Value),
                ReturnedCount  = breakdown.Where(kv => kv.Key.Status == PostStatus.Returned).Sum(kv => kv.Value),
                NewThisMonth   = newPostsThisMonth
            },
            Revenue = new RevenueOverviewStats
            {
                TotalAllTime = revSummary.Total,
                ThisMonth    = revSummary.ThisMonth,
                LastMonth    = revSummary.LastMonth,
                Mrr          = mrr
            },
            Subscriptions = new SubscriptionStats
            {
                TotalActive       = subCounts.UserCount + subCounts.OrgCount,
                UserSubscriptions = subCounts.UserCount,
                OrgSubscriptions  = subCounts.OrgCount
            }
        };
    }
}
