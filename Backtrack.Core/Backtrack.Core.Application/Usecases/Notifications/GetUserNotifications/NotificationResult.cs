using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.Notifications.GetUserNotifications;

public sealed record NotificationResult
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string Body { get; init; } = default!;
    public string Type { get; init; } = default!;
    public string Category { get; init; } = default!;
    public string Status { get; init; } = default!;
    public NotificationData? Data { get; init; }
    public DateTimeOffset SentAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
