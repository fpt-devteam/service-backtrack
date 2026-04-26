namespace Backtrack.Core.Application.Usecases.Devices.RegisterDevice;

public sealed record DeviceResult
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = default!;
    public string DeviceId { get; init; } = default!;
    public bool IsActive { get; init; }
    public DateTimeOffset LastSeenAt { get; init; }
}
