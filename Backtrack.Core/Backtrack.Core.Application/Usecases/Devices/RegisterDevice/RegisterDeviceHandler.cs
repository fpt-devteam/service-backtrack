using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Devices.RegisterDevice;

public sealed class RegisterDeviceHandler : IRequestHandler<RegisterDeviceCommand, DeviceResult>
{
    private readonly IDeviceRepository _deviceRepository;

    public RegisterDeviceHandler(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<DeviceResult> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = new Device
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId!,
            Token = request.Token,
            DeviceId = request.DeviceId,
            IsActive = true,
            LastSeenAt = DateTimeOffset.UtcNow,
        };

        await _deviceRepository.UpsertAsync(device, cancellationToken);
        await _deviceRepository.SaveChangesAsync(cancellationToken);

        return new DeviceResult
        {
            Id = device.Id,
            UserId = device.UserId,
            DeviceId = device.DeviceId,
            IsActive = device.IsActive,
            LastSeenAt = device.LastSeenAt,
        };
    }
}
