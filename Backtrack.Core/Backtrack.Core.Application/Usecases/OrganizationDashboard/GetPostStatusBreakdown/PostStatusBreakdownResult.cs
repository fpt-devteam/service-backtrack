namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetPostStatusBreakdown;

public sealed record PostStatusBreakdownResult
{
    public required StatusBreakdownGroup Org  { get; init; }
    public required StatusBreakdownGroup Mine { get; init; }
}

public sealed record StatusBreakdownGroup
{
    public required PostTypeBreakdown Lost  { get; init; }
    public required PostTypeBreakdown Found { get; init; }
}

public sealed record PostTypeBreakdown
{
    public required int                        Total    { get; init; }
    public required IReadOnlyList<StatusCount> Statuses { get; init; }
}

public sealed record StatusCount
{
    public required string Status { get; init; }
    public required int    Count  { get; init; }
    public required int    Pct    { get; init; }
}
