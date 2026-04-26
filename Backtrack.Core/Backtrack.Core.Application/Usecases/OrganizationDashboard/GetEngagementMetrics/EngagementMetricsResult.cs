namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetEngagementMetrics;

public sealed record EngagementMetricsResult
{
    public required int          Score          { get; init; }
    public required string       Rank           { get; init; }
    public required int          ItemsThisMonth { get; init; }
    public required double       AvgItemsPerDay { get; init; }
    public required List<bool>   WeekActivity   { get; init; }
    public required int          Streak         { get; init; }
}
