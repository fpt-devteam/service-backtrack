namespace Backtrack.Core.Application.Usecases.Admin;

// ── Shared chart primitive ────────────────────────────────────────────────────

public sealed record PeriodCount(string Period, int Count);

public sealed record RevenuePeriodPoint(string Period, decimal Amount, int TransactionCount);

// ── Dashboard overview ────────────────────────────────────────────────────────

public sealed record DashboardOverviewResult
{
    public required UserOverviewStats       Users         { get; init; }
    public required OrgOverviewStats        Organizations { get; init; }
    public required PostOverviewStats       Posts         { get; init; }
    public required RevenueOverviewStats    Revenue       { get; init; }
    public required SubscriptionStats       Subscriptions { get; init; }
}

public sealed record UserOverviewStats
{
    public required int Total        { get; init; }
    public required int Active       { get; init; }
    public required int Inactive     { get; init; }
    public required int NewThisMonth { get; init; }
}

public sealed record OrgOverviewStats
{
    public required int Total        { get; init; }
    public required int Active       { get; init; }
    public required int NewThisMonth { get; init; }
}

public sealed record PostOverviewStats
{
    public required int Total        { get; init; }
    public required int LostCount    { get; init; }
    public required int FoundCount   { get; init; }
    public required int ActiveCount  { get; init; }
    public required int InStorageCount { get; init; }
    public required int ReturnedCount  { get; init; }
    public required int NewThisMonth { get; init; }
}

public sealed record RevenueOverviewStats
{
    public required decimal TotalAllTime { get; init; }
    public required decimal ThisMonth    { get; init; }
    public required decimal LastMonth    { get; init; }
    public required decimal Mrr          { get; init; }
}

public sealed record SubscriptionStats
{
    public required int TotalActive       { get; init; }
    public required int UserSubscriptions { get; init; }
    public required int OrgSubscriptions  { get; init; }
}

// ── Revenue overview ──────────────────────────────────────────────────────────

public sealed record RevenueOverviewResult
{
    public required RevenueOverviewStats Summary       { get; init; }
    public required decimal              Arr           { get; init; }
    public required decimal              UserRevenue   { get; init; }
    public required decimal              OrgRevenue    { get; init; }
    public required SubscriptionStats    Subscriptions { get; init; }
    public required List<RevenuePeriodPoint> Chart     { get; init; }
}

// ── Post detail overview ──────────────────────────────────────────────────────

public sealed record PostDetailOverviewResult
{
    public required int    Total        { get; init; }
    public required int    NewThisMonth { get; init; }
    public required int    LostCount    { get; init; }
    public required int    FoundCount   { get; init; }
    public required PostStatusBreakdown ByStatus  { get; init; }
    public required double              MatchRate { get; init; }
    public required List<PeriodCount>   Chart     { get; init; }
}

public sealed record PostStatusBreakdown
{
    public required int Active          { get; init; }
    public required int InStorage       { get; init; }
    public required int ReturnScheduled { get; init; }
    public required int Returned        { get; init; }
    public required int Archived        { get; init; }
    public required int Expired         { get; init; }
}

// ── Growth chart ──────────────────────────────────────────────────────────────

public sealed record GrowthChartResult
{
    public required string            Entity { get; init; }
    public required int               Months { get; init; }
    public required List<PeriodCount> Points { get; init; }
}
