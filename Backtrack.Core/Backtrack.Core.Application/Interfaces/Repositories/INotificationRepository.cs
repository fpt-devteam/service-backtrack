using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<(Notification Notification, bool Deduped)> CreateAsync(Notification notification, CancellationToken ct = default);
    Task<(IReadOnlyList<Notification> Items, int Total)> GetByUserIdAsync(string userId, int offset, int limit, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);
    Task UpdateStatusAsync(string userId, IEnumerable<Guid> ids, NotificationStatus status, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
