namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetOrgPostStats;

public sealed record OrgPostStatsResult
{
    public required int FoundPosts { get; init; }

    public required int FoundThisMonth { get; init; }
}
