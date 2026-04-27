using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Devices.UnregisterDevice;

public sealed record UnregisterDeviceCommand : IRequest<Unit>
{
    [JsonIgnore]
    public string? UserId { get; init; } = default!;
    public required string DeviceId { get; init; }
}
