namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetOrgPostStats;

public sealed record OrgPostStatsResult
{
    public required int LostPosts  { get; init; }
    public required int FoundPosts { get; init; }
    public required int Total      { get; init; }
    public required ThisMonthStats ThisMonth { get; init; }
}

public sealed record ThisMonthStats
{
    public required int Lost  { get; init; }
    public required int Found { get; init; }
}
