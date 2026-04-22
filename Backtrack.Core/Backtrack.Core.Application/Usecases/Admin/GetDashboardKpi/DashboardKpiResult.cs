namespace Backtrack.Core.Application.Usecases.Admin.GetDashboardKpi;

public sealed record DashboardKpiResult
{
    public required string          Period      { get; init; }
    public required DateTimeOffset  GeneratedAt { get; init; }
    public required KpiPostMetric   TotalLostItems    { get; init; }
    public required KpiPostMetric   TotalFound        { get; init; }
    public required KpiReturnRate   SuccessReturnRate { get; init; }
    public required KpiRevenueMetric RevenueThisMonth { get; init; }
}

public sealed record KpiPostMetric
{
    public required int          Value         { get; init; }
    public required double       ChangePercent { get; init; }
    public required int          ThisMonth     { get; init; }
    public required List<int>    Sparkline     { get; init; }
}

public sealed record KpiReturnRate
{
    public required int    Value         { get; init; }
    public required double ChangePercent { get; init; }
    public required int    Returned      { get; init; }
    public required int    Total         { get; init; }
}

public sealed record KpiRevenueMetric
{
    public required decimal      Value         { get; init; }
    public required double       ChangePercent { get; init; }
    public required decimal      VsLastMonth   { get; init; }
    public required List<decimal> Sparkline    { get; init; }
}
