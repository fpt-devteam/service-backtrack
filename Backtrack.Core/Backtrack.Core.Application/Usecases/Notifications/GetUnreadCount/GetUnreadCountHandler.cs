using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Notifications.GetUnreadCount;

public sealed class GetUnreadCountHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _notificationRepository;

    public GetUnreadCountHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        return await _notificationRepository.GetUnreadCountAsync(request.UserId, cancellationToken);
    }
}
