using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Domain.Entities;

public sealed class Notification : Entity<Guid>
{
    public string UserId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
    public NotificationEvent Type { get; set; }
    public NotificationCategory Category { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public NotificationData? Data { get; set; }
    public string SourceName { get; set; } = default!;
    public string SourceEventId { get; set; } = default!;
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = default!;
}
