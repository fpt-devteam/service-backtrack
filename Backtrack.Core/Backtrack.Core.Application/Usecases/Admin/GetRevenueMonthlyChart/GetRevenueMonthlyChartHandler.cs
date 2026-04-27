using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueMonthlyChart;

public sealed class GetRevenueMonthlyChartHandler(
    IUserRepository           userRepository,
    IPaymentHistoryRepository paymentHistoryRepository)
    : IRequestHandler<GetRevenueMonthlyChartQuery, List<MonthlyRevenueChartItem>>
{
    private static readonly string[] MonthAbbr =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    public async Task<List<MonthlyRevenueChartItem>> Handle(
        GetRevenueMonthlyChartQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var rows = await paymentHistoryRepository.GetRevenueMonthlyAsync(query.Months, cancellationToken);

        var now   = DateTimeOffset.UtcNow;
        var start = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero)
                        .AddMonths(-(query.Months - 1));

        var lookup = rows.ToDictionary(r => (r.Year, r.Month), r => (r.Org, r.User));

        return Enumerable.Range(0, query.Months)
            .Select(i => start.AddMonths(i))
            .Select(p =>
            {
                var (org, user) = lookup.TryGetValue((p.Year, p.Month), out var v) ? v : (0m, 0m);
                return new MonthlyRevenueChartItem(
                    Month:        MonthAbbr[p.Month - 1],
                    Subscription: org,
                    QrSales:      user,
                    Total:        org + user);
            })
            .ToList();
    }
}
