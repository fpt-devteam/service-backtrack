using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Devices.RegisterDevice;

public sealed record RegisterDeviceCommand : IRequest<DeviceResult>
{
    [JsonIgnore]
    public string? UserId { get; init; } = default!;
    public required string Token { get; init; }
    public required string DeviceId { get; init; }
}
