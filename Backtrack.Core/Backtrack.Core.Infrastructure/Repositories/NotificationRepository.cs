using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(Notification Notification, bool Deduped)> CreateAsync(Notification notification, CancellationToken ct = default)
    {
        var existing = await _context.Notifications
            .FirstOrDefaultAsync(n => n.SourceName == notification.SourceName && n.SourceEventId == notification.SourceEventId, ct);

        if (existing is not null)
            return (existing, true);

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);
        return (notification, false);
    }

    public async Task<(IReadOnlyList<Notification> Items, int Total)> GetByUserIdAsync(string userId, int offset, int limit, CancellationToken ct = default)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip(offset).Take(limit).ToListAsync(ct);
        return (items, total);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && n.Status == NotificationStatus.Unread, ct);
    }

    public async Task UpdateStatusAsync(string userId, IEnumerable<Guid> ids, NotificationStatus status, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        await _context.Notifications
            .Where(n => n.UserId == userId && idList.Contains(n.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Status, status)
                .SetProperty(n => n.UpdatedAt, DateTimeOffset.UtcNow), ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
