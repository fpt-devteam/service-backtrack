namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetOrgPostStats;

public sealed record OrgPostStatsResult
{
    public required int FoundPosts { get; init; }

    public required ThisMonthStats ThisMonth { get; init; }
}

public sealed record ThisMonthStats
{
    public required int Found { get; init; }
}
