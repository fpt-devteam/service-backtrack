using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class OrgReturnReportRepository : CrudRepositoryBase<OrgReturnReport, Guid>, IOrgReturnReportRepository
{
    public OrgReturnReportRepository(ApplicationDbContext context) : base(context) { }

    public async Task<OrgReturnReport?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OrgReturnReport>()
            .Include(r => r.Organization)
            .Include(r => r.Staff)
            .Include(r => r.Post)
                .ThenInclude(p => p!.Author)
            .FirstOrDefaultAsync(r => r.Id == id && r.DeletedAt == null, cancellationToken);
    }

    public async Task<bool> ExistsActiveForPostAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OrgReturnReport>()
            .AnyAsync(r => r.PostId == postId && r.DeletedAt == null, cancellationToken);
    }

    public async Task<(List<OrgReturnReport> Items, int Total)> GetByOrgAsync(
        Guid orgId, string? staffId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<OrgReturnReport>()
            .Include(r => r.Staff)
            .Include(r => r.Post)
                .ThenInclude(p => p!.Author)
            .Where(r => r.OrgId == orgId && r.DeletedAt == null);

        if (staffId != null)
            query = query.Where(r => r.StaffId == staffId);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Dictionary<Guid, OrgReturnReport>> GetByPostIdsAsync(
        IEnumerable<Guid> postIds, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OrgReturnReport>()
            .Where(r => postIds.Contains(r.PostId) && r.DeletedAt == null)
            .ToDictionaryAsync(r => r.PostId, cancellationToken);
    }

    public async Task<OrgReturnReport?> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OrgReturnReport>()
            .FirstOrDefaultAsync(r => r.PostId == postId && r.DeletedAt == null, cancellationToken);
    }

    public async Task<int> CountPendingByStaffAsync(Guid orgId, string staffId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OrgReturnReport>()
            .CountAsync(r => r.OrgId == orgId && r.StaffId == staffId && r.DeletedAt == null, cancellationToken);
    }

    public async Task<int> CountByOrgSinceAsync(Guid orgId, DateTimeOffset since, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OrgReturnReport>()
            .CountAsync(r => r.OrgId == orgId && r.DeletedAt == null && r.CreatedAt >= since, cancellationToken);
    }
}
