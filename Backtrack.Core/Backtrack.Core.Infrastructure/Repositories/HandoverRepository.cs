using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class HandoverRepository : CrudRepositoryBase<Handover, Guid>, IHandoverRepository
{
    public HandoverRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Handover?> GetByIdWithExtensionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(h => h.OrgExtension)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<Handover?> GetByIdWithPostsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(h => h.FinderPost)
                .ThenInclude(p => p!.Author)
            .Include(h => h.OwnerPost)
                .ThenInclude(p => p!.Author)
            .Include(h => h.OrgExtension)
                .ThenInclude(e => e!.Organization)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<List<Handover>> GetExpiredPendingHandoversAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _dbSet
            .Where(h => h.Status == HandoverStatus.Pending && h.ExpiresAt <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Handover> Items, int Total)> GetByUserAsync(
        string userId,
        int page,
        int pageSize,
        HandoverStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(h => h.FinderPost)
            .Include(h => h.OwnerPost)
            .Where(h => (h.FinderPost != null && h.FinderPost.AuthorId == userId) ||
                       (h.OwnerPost != null && h.OwnerPost.AuthorId == userId));

        if (status.HasValue)
        {
            query = query.Where(h => h.Status == status.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<bool> ExistsActiveHandoverForPostsAsync(
        Guid? finderPostId,
        Guid? ownerPostId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(h =>
            h.FinderPostId == finderPostId &&
            h.OwnerPostId == ownerPostId &&
            h.Status == HandoverStatus.Pending,
            cancellationToken);
    }
}
