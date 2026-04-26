using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Notifications.UpdateNotificationStatus;

public sealed class UpdateNotificationStatusHandler : IRequestHandler<UpdateNotificationStatusCommand, Unit>
{
    private readonly INotificationRepository _notificationRepository;

    public UpdateNotificationStatusHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Unit> Handle(UpdateNotificationStatusCommand request, CancellationToken cancellationToken)
    {
        await _notificationRepository.UpdateStatusAsync(request.UserId, request.NotificationIds, request.Status, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
