using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Notifications.GetUserNotifications;

public sealed class GetUserNotificationsHandler : IRequestHandler<GetUserNotificationsQuery, (IReadOnlyList<NotificationResult> Items, int Total)>
{
    private readonly INotificationRepository _notificationRepository;

    public GetUserNotificationsHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<(IReadOnlyList<NotificationResult> Items, int Total)> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var offset = (request.Page - 1) * request.PageSize;
        var (items, total) = await _notificationRepository.GetByUserIdAsync(request.UserId, offset, request.PageSize, cancellationToken);

        var results = items.Select(n => new NotificationResult
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            Body = n.Body,
            Type = n.Type.ToString(),
            Category = n.Category.ToString(),
            Status = n.Status.ToString(),
            Data = n.Data,
            SentAt = n.SentAt,
            CreatedAt = n.CreatedAt,
        }).ToList();

        return (results, total);
    }
}
