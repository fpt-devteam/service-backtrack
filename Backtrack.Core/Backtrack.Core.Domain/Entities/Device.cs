namespace Backtrack.Core.Domain.Entities;

public sealed class Device : Entity<Guid>
{
    public string UserId { get; set; } = default!;
    public string Token { get; set; } = default!;
    public string DeviceId { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = default!;
}
