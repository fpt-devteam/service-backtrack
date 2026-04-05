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
        // var orgReturnReport = await _context.Set<OrgReturnReport>()
        //     .Include(h => h.Organization)
        //     .Include(h => h.Staff)
        //     .Include(h => h.Finder)
        //     .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        // if (orgReturnReport != null)
        // {
        //     return orgReturnReport;
        // }

        return await _context.Set<C2CReturnReport>()
            .Include(h => h.Finder)
            .Include(h => h.Owner)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<C2CReturnReport?> GetByIdWithPostsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var c2cReturnReport = await _context.Set<C2CReturnReport>()
            .Include(h => h.FinderPost)
                .ThenInclude(p => p!.Author)
            .Include(h => h.OwnerPost)
                .ThenInclude(p => p!.Author)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (c2cReturnReport == null)
        {
            return null;
        }
        return c2cReturnReport;

        // return await _context.Set<OrgReturnReport>()
        //     .Include(h => h.Post)
        //         .ThenInclude(p => p!.Author)
        //     .Include(h => h.Organization)
        //     .Include(h => h.Staff)
        //     .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<List<C2CReturnReport>> GetExpiredPendingReturnReportsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _dbSet
            .Where(h => h.Status == ReturnReportStatus.Pending && h.ExpiresAt <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<C2CReturnReport> Items, int Total)> GetByUserAsync(
        string userId,
        int page,
        int pageSize,
        ReturnReportStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<C2CReturnReport> c2cQuery = _context.Set<C2CReturnReport>()
            .Include(h => h.FinderPost)
            .Include(h => h.OwnerPost)
            .Where(h => (h.FinderPost != null && h.FinderPost.AuthorId == userId) ||
                       (h.OwnerPost != null && h.OwnerPost.AuthorId == userId));

        if (status.HasValue)
        {
            c2cQuery = c2cQuery.Where(h => h.Status == status.Value);
        }

        var total = await c2cQuery.CountAsync(cancellationToken);

        var allItems = (await c2cQuery.Cast<C2CReturnReport>().ToListAsync(cancellationToken));

        var items = allItems
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, total);
    }

    public async Task<bool> ExistsActiveReturnReportForPostsAsync(
        Guid finderPostId,
        Guid ownerPostId,
        CancellationToken cancellationToken = default)
    {
        var c2cExists = await _context.Set<C2CReturnReport>().AnyAsync(h =>
            h.FinderPostId == finderPostId &&
            h.OwnerPostId == ownerPostId,
            cancellationToken);

        return c2cExists;
    }
}
