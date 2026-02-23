namespace Backtrack.Core.Application.Usecases.Organizations.Queries.CheckInvitation;

public sealed record CheckInvitationResult
{
    public required bool IsTokenValid { get; init; }
    public string? OrganizationName { get; init; }
    public string? Role { get; init; }
}
