namespace Backtrack.Core.Application.Usecases.Organizations.JoinByInvitation;

public sealed record JoinByInvitationResult
{
    public required Guid OrganizationId { get; init; }
    public required Guid MembershipId { get; init; }
    public required string Role { get; init; }
}
