namespace Backtrack.Core.Application.Usecases.Admin.GetOrgDashboardStats;

public sealed record OrgDashboardStatsResult
{
    public required int TotalStaff        { get; init; }
    public required int ActiveStaff       { get; init; }
    public required int NewStaffThisMonth { get; init; }
    public required int TotalItems        { get; init; }
    public required int InStorage         { get; init; }
    public required int ReturnedThisMonth { get; init; }
    public required int ExpiredItems      { get; init; }
    public required int ReturnRate        { get; init; }
    public required int FoundToday        { get; init; }
    public required int LostPosts         { get; init; }
    public required int FoundPosts        { get; init; }
}
