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
        var orgHandover = await _context.Set<OrgHandover>()
            .Include(h => h.Organization)
            .Include(h => h.Staff)
            .Include(h => h.Finder)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (orgHandover != null)
        {
            return orgHandover;
        }

        return await _context.Set<P2PHandover>()
            .Include(h => h.Finder)
            .Include(h => h.Owner)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<Handover?> GetByIdWithPostsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var p2pHandover = await _context.Set<P2PHandover>()
            .Include(h => h.FinderPost)
                .ThenInclude(p => p!.Author)
            .Include(h => h.OwnerPost)
                .ThenInclude(p => p!.Author)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (p2pHandover != null)
        {
            return p2pHandover;
        }

        return await _context.Set<OrgHandover>()
            .Include(h => h.FinderPost)
                .ThenInclude(p => p!.Author)
            .Include(h => h.Organization)
            .Include(h => h.Staff)
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
        IQueryable<P2PHandover> p2pQuery = _context.Set<P2PHandover>()
            .Include(h => h.FinderPost)
            .Include(h => h.OwnerPost)
            .Where(h => (h.FinderPost != null && h.FinderPost.AuthorId == userId) ||
                       (h.OwnerPost != null && h.OwnerPost.AuthorId == userId));

        if (status.HasValue)
        {
            p2pQuery = p2pQuery.Where(h => h.Status == status.Value);
        }

        IQueryable<OrgHandover> orgQuery = _context.Set<OrgHandover>()
            .Include(h => h.FinderPost)
            .Where(h => h.FinderPost != null && h.FinderPost.AuthorId == userId);

        if (status.HasValue)
        {
            orgQuery = orgQuery.Where(h => h.Status == status.Value);
        }

        var total = await p2pQuery.CountAsync(cancellationToken) + await orgQuery.CountAsync(cancellationToken);

        var allItems = (await p2pQuery.Cast<Handover>().ToListAsync(cancellationToken))
            .Concat(await orgQuery.Cast<Handover>().ToListAsync(cancellationToken));

        var items = allItems
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, total);
    }

    public async Task<bool> ExistsActiveHandoverForPostsAsync(
        Guid? finderPostId,
        Guid? ownerPostId,
        CancellationToken cancellationToken = default)
    {
        var p2pExists = await _context.Set<P2PHandover>().AnyAsync(h =>
            h.FinderPostId == finderPostId &&
            h.OwnerPostId == ownerPostId,
            cancellationToken);

        if (ownerPostId.HasValue)
        {
            return p2pExists;
        }

        var orgExists = await _context.Set<OrgHandover>().AnyAsync(h =>
            h.FinderPostId == finderPostId,
            cancellationToken);

        return p2pExists || orgExists;
    }
}
