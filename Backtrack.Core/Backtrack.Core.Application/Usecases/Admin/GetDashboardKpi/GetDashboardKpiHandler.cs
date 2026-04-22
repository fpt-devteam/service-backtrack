using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetDashboardKpi;

public sealed class GetDashboardKpiHandler(
    IUserRepository           userRepository,
    IPostRepository           postRepository,
    IPaymentHistoryRepository paymentHistoryRepository) : IRequestHandler<GetDashboardKpiQuery, DashboardKpiResult>
{
    public async Task<DashboardKpiResult> Handle(GetDashboardKpiQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var now           = DateTimeOffset.UtcNow;
        var startOfMonth  = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var thisMonthKey  = startOfMonth.ToString("yyyy-MM");
        var lastMonthKey  = startOfMonth.AddMonths(-1).ToString("yyyy-MM");

        var breakdown   = await postRepository.GetBreakdownAsync(cancellationToken);
        var monthlyPost = await postRepository.GetMonthlyPostKpiAsync(12, cancellationToken);
        var revSummary  = await paymentHistoryRepository.GetRevenueSummaryAsync(cancellationToken);
        var revChart    = await paymentHistoryRepository.GetRevenueChartAsync(12, cancellationToken);

        // Totals from breakdown
        int totalLost     = breakdown.Where(kv => kv.Key.Type == PostType.Lost).Sum(kv => kv.Value);
        int totalFound    = breakdown.Where(kv => kv.Key.Type == PostType.Found).Sum(kv => kv.Value);
        int totalReturned = breakdown.Where(kv => kv.Key.Status == PostStatus.Returned).Sum(kv => kv.Value);

        // Monthly lookup: period → list of (PostType, Status, Count)
        var monthlyByPeriod = monthlyPost
            .GroupBy(x => x.Period)
            .ToDictionary(g => g.Key, g => g.ToList());

        int MonthlyTypeCount(string period, PostType type) =>
            monthlyByPeriod.TryGetValue(period, out var rows)
                ? rows.Where(r => r.PostType == type).Sum(r => r.Count)
                : 0;

        int MonthlyReturnedCount(string period) =>
            monthlyByPeriod.TryGetValue(period, out var rows)
                ? rows.Where(r => r.Status == PostStatus.Returned).Sum(r => r.Count)
                : 0;

        int lostThisMonth     = MonthlyTypeCount(thisMonthKey, PostType.Lost);
        int lostLastMonth     = MonthlyTypeCount(lastMonthKey, PostType.Lost);
        int foundThisMonth    = MonthlyTypeCount(thisMonthKey, PostType.Found);
        int foundLastMonth    = MonthlyTypeCount(lastMonthKey, PostType.Found);
        int returnedThisMonth = MonthlyReturnedCount(thisMonthKey);
        int returnedLastMonth = MonthlyReturnedCount(lastMonthKey);

        // Sparkline periods: last 12 months in ascending order
        var periods = Enumerable.Range(0, 12)
            .Select(i => startOfMonth.AddMonths(-(11 - i)).ToString("yyyy-MM"))
            .ToList();

        List<int> PostSparkline(PostType type) =>
            periods.Select(p => MonthlyTypeCount(p, type)).ToList();

        var revChartDict   = revChart.ToDictionary(x => x.Period, x => x.Amount);
        List<decimal> RevenueSparkline() =>
            periods.Select(p => revChartDict.TryGetValue(p, out var v) ? v : 0m).ToList();

        // Success return rate
        double thisMonthRate = foundThisMonth > 0 ? (double)returnedThisMonth / foundThisMonth * 100 : 0;
        double lastMonthRate = foundLastMonth > 0 ? (double)returnedLastMonth / foundLastMonth * 100 : 0;
        int    returnRate    = totalFound > 0 ? (int)Math.Round((double)totalReturned / totalFound * 100) : 0;

        // Revenue
        decimal revenueThis = revSummary.ThisMonth;
        decimal revenueLast = revSummary.LastMonth;

        return new DashboardKpiResult
        {
            Period      = thisMonthKey,
            GeneratedAt = now,
            TotalLostItems = new KpiPostMetric
            {
                Value         = totalLost,
                ChangePercent = CalcChange(lostLastMonth, lostThisMonth),
                ThisMonth     = lostThisMonth,
                Sparkline     = PostSparkline(PostType.Lost)
            },
            TotalFound = new KpiPostMetric
            {
                Value         = totalFound,
                ChangePercent = CalcChange(foundLastMonth, foundThisMonth),
                ThisMonth     = foundThisMonth,
                Sparkline     = PostSparkline(PostType.Found)
            },
            SuccessReturnRate = new KpiReturnRate
            {
                Value         = returnRate,
                ChangePercent = CalcChange(lastMonthRate, thisMonthRate),
                Returned      = totalReturned,
                Total         = totalFound
            },
            RevenueThisMonth = new KpiRevenueMetric
            {
                Value         = revenueThis,
                ChangePercent = CalcChange((double)revenueLast, (double)revenueThis),
                VsLastMonth   = revenueThis - revenueLast,
                Sparkline     = RevenueSparkline()
            }
        };
    }

    private static double CalcChange(double previous, double current)
        => previous == 0 ? 0 : Math.Round((current - previous) / previous * 100, 1);
}
