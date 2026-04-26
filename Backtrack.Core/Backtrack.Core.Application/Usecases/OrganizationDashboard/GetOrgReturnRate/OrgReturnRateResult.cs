namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetOrgReturnRate;

public sealed record OrgReturnRateResult
{
    public required int    Returned   { get; init; }
    public required int    InStorage  { get; init; }
    public required int    Expired    { get; init; }
    public required int    Other      { get; init; }
    public required int    Total      { get; init; }
    public required double ReturnRate { get; init; }
}
