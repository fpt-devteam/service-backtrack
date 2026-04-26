namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetMyReturnRate;

public sealed record MyReturnRateResult
{
    public required int    Returned   { get; init; }
    public required int    InStorage  { get; init; }
    public required int    Expired    { get; init; }
    public required int    Other      { get; init; }
    public required int    Total      { get; init; }
    public required double ReturnRate { get; init; }
}
