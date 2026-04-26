namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetStaffDashboardStats;

public sealed record StaffDashboardStatsResult
{
    public required int MyItemsInStorage { get; init; }
    public required int MyItemsTotal     { get; init; }
    public required int PendingReturns   { get; init; }
    public required int ReturnedThisWeek { get; init; }
}
