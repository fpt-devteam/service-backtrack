using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Notifications.UpdateNotificationStatus;

public sealed record UpdateNotificationStatusCommand : IRequest<Unit>
{
    public string UserId { get; init; } = default!;
    public required IEnumerable<Guid> NotificationIds { get; init; }
    public NotificationStatus Status { get; init; } = NotificationStatus.Read;
}
