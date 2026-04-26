using MediatR;

namespace Backtrack.Core.Application.Usecases.Notifications.GetUnreadCount;

public sealed record GetUnreadCountQuery : IRequest<int>
{
    public string UserId { get; init; } = default!;
}
