namespace Backtrack.Core.Application.Usecases.Admin.GetOrgStaffPerformance;

public sealed record StaffPerformanceItemResult
{
    public required string Id           { get; init; }
    public required string Name         { get; init; }
    public required string Role         { get; init; }
    public required int    ItemsLogged  { get; init; }
    public required int    ReturnRate   { get; init; }
    public required int    ActiveChats  { get; init; }
    public required int    Streak       { get; init; }
}
