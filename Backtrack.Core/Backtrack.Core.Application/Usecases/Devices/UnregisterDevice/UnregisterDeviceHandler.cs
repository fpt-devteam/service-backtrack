using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Devices.UnregisterDevice;

public sealed class UnregisterDeviceHandler : IRequestHandler<UnregisterDeviceCommand, Unit>
{
    private readonly IDeviceRepository _deviceRepository;

    public UnregisterDeviceHandler(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<Unit> Handle(UnregisterDeviceCommand request, CancellationToken cancellationToken)
    {
        var found = await _deviceRepository.DeactivateAsync(request.UserId, request.DeviceId, cancellationToken);
        if (!found)
            throw new NotFoundException(NotificationErrors.DeviceNotFound);

        await _deviceRepository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
