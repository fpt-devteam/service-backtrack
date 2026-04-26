using MediatR;

namespace Backtrack.Core.Application.Usecases.Notifications.GetUserNotifications;

public sealed record GetUserNotificationsQuery : IRequest<(IReadOnlyList<NotificationResult> Items, int Total)>
{
    public string UserId { get; init; } = default!;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
