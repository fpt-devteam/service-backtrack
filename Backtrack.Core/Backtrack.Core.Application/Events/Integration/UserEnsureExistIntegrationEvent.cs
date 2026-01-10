namespace Backtrack.Core.Application.Events.Integration;

public sealed class UserEnsureExistIntegrationEvent
{
    public required string Id { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public required string GlobalRole { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset EventTimestamp { get; set; }
}
