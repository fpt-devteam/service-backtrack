namespace Backtrack.Core.Application.Events.Integration;

public sealed class UserCreatedIntegrationEvent
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public string? DisplayName { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset EventTimestamp { get; set; }
}
