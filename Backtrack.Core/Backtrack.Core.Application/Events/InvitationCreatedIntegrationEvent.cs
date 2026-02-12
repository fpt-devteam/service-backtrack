namespace Backtrack.Core.Application.Events;

public sealed class InvitationCreatedIntegrationEvent
{
    public required Guid InvitationId { get; set; }
    public required string Email { get; set; }
    public required string OrganizationName { get; set; }
    public required string InviterName { get; set; }
    public required string Role { get; set; }
    public required string HashCode { get; set; }
    public required DateTimeOffset ExpiredTime { get; set; }
    public required DateTimeOffset EventTimestamp { get; set; }
}
