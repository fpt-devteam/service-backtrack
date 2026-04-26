using MediatR;

namespace Backtrack.Core.Application.Usecases.Devices.UnregisterDevice;

public sealed record UnregisterDeviceCommand : IRequest<Unit>
{
    public string UserId { get; init; } = default!;
    public required string DeviceId { get; init; }
}
