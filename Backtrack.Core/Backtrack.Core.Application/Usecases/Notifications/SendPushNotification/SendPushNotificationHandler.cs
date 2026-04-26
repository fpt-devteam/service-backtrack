using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Interfaces.PushNotification;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Notifications.SendPushNotification;

public sealed class SendPushNotificationHandler : IRequestHandler<SendPushNotificationCommand, SendPushNotificationResult>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IPushNotificationService _pushNotificationService;

    public SendPushNotificationHandler(
        INotificationRepository notificationRepository,
        IDeviceRepository deviceRepository,
        IPushNotificationService pushNotificationService)
    {
        _notificationRepository = notificationRepository;
        _deviceRepository = deviceRepository;
        _pushNotificationService = pushNotificationService;
    }

    public async Task<SendPushNotificationResult> Handle(SendPushNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId!,
            Title = request.Title,
            Body = request.Body,
            Type = request.Type,
            Category = request.Category,
            Data = request.Data,
            SourceName = request.Source.Name,
            SourceEventId = string.IsNullOrEmpty(request.Source.EventId) ? Guid.NewGuid().ToString() : request.Source.EventId,
            SentAt = DateTimeOffset.UtcNow,
        };

        var (created, deduped) = await _notificationRepository.CreateAsync(notification, cancellationToken);

        if (!deduped && request.Category == NotificationCategory.Push)
        {
            var devices = await _deviceRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken);
            var tokens = devices.Select(d => d.Token).ToList();
            if (tokens.Count > 0)
                await _pushNotificationService.SendAsync(tokens, request.Title, request.Body, request.Data, cancellationToken);
        }

        return new SendPushNotificationResult { Success = true };
    }
}
