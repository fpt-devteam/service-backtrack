using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class MembershipRepository : CrudRepositoryBase<Membership, Guid>, IMembershipRepository
{
    public MembershipRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Membership?> GetByOrgAndUserAsync(Guid orgId, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.OrganizationId == orgId && m.UserId == userId, cancellationToken);
    }

    public async Task<(IEnumerable<Membership> Items, int TotalCount)> GetPagedByOrgAsync(
        Guid orgId, int offset, int limit, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(m => m.OrganizationId == orgId)
            .Include(m => m.User)
            .OrderBy(m => m.JoinedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(offset).Take(limit).ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IEnumerable<Membership>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.UserId == userId && m.Status == MembershipStatus.Active)
            .Include(m => m.Organization)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountActiveAdminsAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(m =>
                m.OrganizationId == orgId &&
                m.Role == MembershipRole.OrgAdmin &&
                m.Status == MembershipStatus.Active,
                cancellationToken);
    }

    public async Task<Membership?> GetByOrgAndUserEmailAsync(Guid orgId, string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.OrganizationId == orgId && m.User.Email == email, cancellationToken);
    }

    public async Task<int> CountByOrgAsync(Guid orgId, CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(m => m.OrganizationId == orgId, cancellationToken);

    public async Task<int> CountActiveByOrgAsync(Guid orgId, CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(m => m.OrganizationId == orgId && m.Status == MembershipStatus.Active, cancellationToken);

    public async Task<int> CountNewByOrgSinceAsync(Guid orgId, DateTimeOffset since, CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(m => m.OrganizationId == orgId && m.JoinedAt >= since, cancellationToken);

    public async Task<List<Membership>> GetByOrgIdsWithUserAsync(
        IEnumerable<Guid> orgIds,
        CancellationToken cancellationToken = default)
    {
        var ids = orgIds.ToList();
        return await _dbSet.AsNoTracking()
            .Include(m => m.User)
            .Where(m => ids.Contains(m.OrganizationId))
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetCountsByOrgIdsAsync(
        IEnumerable<Guid> orgIds,
        CancellationToken cancellationToken = default)
    {
        var ids  = orgIds.ToList();
        var rows = await _dbSet.AsNoTracking()
            .Where(m => ids.Contains(m.OrganizationId))
            .GroupBy(m => m.OrganizationId)
            .Select(g => new { OrgId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(r => r.OrgId, r => r.Count);
    }
}
