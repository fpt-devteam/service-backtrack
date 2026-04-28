namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueSummary;

public sealed record RevenueSummaryResult
{
    public required decimal TotalRevenue             { get; init; }
    public required decimal MonthlyRevenue           { get; init; }
    public required decimal GrowthPercentage         { get; init; }
    public required int     TotalTransactions        { get; init; }
    public required decimal AverageTransactionValue  { get; init; }
    public required decimal SubscriptionRevenue      { get; init; }
    public required decimal QrSalesRevenue           { get; init; }
    public required int     SubscriptionTransactions { get; init; }
    public required int     QrSalesTransactions      { get; init; }
}
