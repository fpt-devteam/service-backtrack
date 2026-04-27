namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueMonthly;

public sealed record RevenueMonthlyItemResult(
    string  Month,
    int     Year,
    decimal Org,
    decimal User);
