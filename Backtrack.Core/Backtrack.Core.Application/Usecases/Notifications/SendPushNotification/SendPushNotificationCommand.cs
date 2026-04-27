using System.Text.Json.Serialization;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Notifications.SendPushNotification;

public sealed record NotificationSource
{
    public required string Name { get; init; }
    public required string EventId { get; init; }
}

public sealed record NotificationTarget
{
    public required string UserId { get; init; }
}

public sealed record SendPushNotificationCommand : IRequest<SendPushNotificationResult>
{
    public required NotificationTarget Target { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public NotificationEvent Type { get; init; } = NotificationEvent.SystemAlertEvent;
    public NotificationCategory Category { get; init; } = NotificationCategory.Push;
    public NotificationData? Data { get; init; }
    public required NotificationSource Source { get; init; }
}
