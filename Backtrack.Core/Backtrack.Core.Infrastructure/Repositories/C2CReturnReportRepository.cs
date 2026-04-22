using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class ReturnReportRepository : CrudRepositoryBase<C2CReturnReport, Guid>, IC2CReturnReportRepository
{
    public ReturnReportRepository(ApplicationDbContext context) : base(context) { }

    public async Task<C2CReturnReport?> GetByIdWithExtensionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<C2CReturnReport>()
            .Include(h => h.Finder)
            .Include(h => h.Owner)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<C2CReturnReport?> GetByIdWithPostsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<C2CReturnReport>()
            .Include(h => h.Finder)
            .Include(h => h.Owner)
            .Include(h => h.FinderPost)
                .ThenInclude(p => p!.Author)
            .Include(h => h.OwnerPost)
                .ThenInclude(p => p!.Author)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    // public async Task<List<C2CReturnReport>> GetExpiredPendingReturnReportsAsync(CancellationToken cancellationToken = default)
    // {
    //     var now = DateTimeOffset.UtcNow;
    //     return await _dbSet
    //         .Where(h => h.Status == C2CReturnReportStatus.Pending && h.ExpiresAt <= now)
    //         .ToListAsync(cancellationToken);
    // }

    public async Task<(List<C2CReturnReport> Items, int Total)> GetByUserAsync(
        string userId,
        int page,
        int pageSize,
        C2CReturnReportStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<C2CReturnReport> c2cQuery = _context.Set<C2CReturnReport>()
            .Include(h => h.Finder)
            .Include(h => h.Owner)
            .Include(h => h.FinderPost)
                .ThenInclude(p => p!.Author)
            .Include(h => h.OwnerPost)
                .ThenInclude(p => p!.Author)
            .Where(h => h.FinderId == userId || h.OwnerId == userId);

        if (status.HasValue)
        {
            c2cQuery = c2cQuery.Where(h => h.Status == status.Value);
        }

        var total = await c2cQuery.CountAsync(cancellationToken);

        var items = await c2cQuery
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<List<C2CReturnReport>> GetByPartnerAsync(
        string userId,
        string partnerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<C2CReturnReport>()
            .Include(h => h.Finder)
            .Include(h => h.Owner)
            .Include(h => h.FinderPost)
                .ThenInclude(p => p!.Author)
            .Include(h => h.OwnerPost)
                .ThenInclude(p => p!.Author)
            .Where(h => (h.FinderId == userId && h.OwnerId == partnerId) ||
                        (h.FinderId == partnerId && h.OwnerId == userId))
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<C2CReturnReport?> GetOngoingByParticipantsAndPostsAsync(
        string finderId,
        string ownerId,
        Guid finderPostId,
        Guid ownerPostId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(h => h.Finder)
            .Include(h => h.Owner)
            .Include(h => h.FinderPost)
                .ThenInclude(p => p!.Author)
            .Include(h => h.OwnerPost)
                .ThenInclude(p => p!.Author)
            .FirstOrDefaultAsync(h =>
                h.FinderId == finderId &&
                h.OwnerId == ownerId &&
                h.FinderPostId == finderPostId &&
                h.OwnerPostId == ownerPostId &&
                h.Status == C2CReturnReportStatus.Ongoing,
                cancellationToken);
    }
}
