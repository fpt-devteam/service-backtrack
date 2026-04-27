namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueMonthlyChart;

public sealed record MonthlyRevenueChartItem(
    string  Month,
    decimal Subscription,
    decimal QrSales,
    decimal Total);
