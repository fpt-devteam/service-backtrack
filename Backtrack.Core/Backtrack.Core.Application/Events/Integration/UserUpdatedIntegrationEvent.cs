namespace Backtrack.Core.Application.Events.Integration;

public sealed class UserUpdatedIntegrationEvent
{
    public required string Id { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }
    public required DateTimeOffset EventTimestamp { get; set; }
}
