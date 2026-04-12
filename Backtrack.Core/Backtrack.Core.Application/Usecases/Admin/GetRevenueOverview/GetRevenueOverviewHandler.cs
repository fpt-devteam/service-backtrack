using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueOverview;

public sealed class GetRevenueOverviewHandler(
    IUserRepository           userRepository,
    IPaymentHistoryRepository paymentHistoryRepository,
    ISubscriptionRepository   subscriptionRepository) : IRequestHandler<GetRevenueOverviewQuery, RevenueOverviewResult>
{
    public async Task<RevenueOverviewResult> Handle(GetRevenueOverviewQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var (revSummary, chartData, subCounts, mrr) = (
            await paymentHistoryRepository.GetRevenueSummaryAsync(cancellationToken),
            await paymentHistoryRepository.GetRevenueChartAsync(query.Months, cancellationToken),
            await subscriptionRepository.GetActiveCountsAsync(cancellationToken),
            await subscriptionRepository.GetMrrAsync(cancellationToken)
        );

        return new RevenueOverviewResult
        {
            Summary = new RevenueOverviewStats
            {
                TotalAllTime = revSummary.Total,
                ThisMonth    = revSummary.ThisMonth,
                LastMonth    = revSummary.LastMonth,
                Mrr          = mrr
            },
            Arr         = mrr * 12,
            UserRevenue = revSummary.UserRevenue,
            OrgRevenue  = revSummary.OrgRevenue,
            Subscriptions = new SubscriptionStats
            {
                TotalActive       = subCounts.UserCount + subCounts.OrgCount,
                UserSubscriptions = subCounts.UserCount,
                OrgSubscriptions  = subCounts.OrgCount
            },
            Chart = chartData.Select(c => new RevenuePeriodPoint(c.Period, c.Amount, c.TransactionCount)).ToList()
        };
    }
}
