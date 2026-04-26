using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Interfaces.PushNotification;

public interface IPushNotificationService
{
    Task SendAsync(IEnumerable<string> tokens, string title, string body, NotificationData? data = null, CancellationToken ct = default);
}
