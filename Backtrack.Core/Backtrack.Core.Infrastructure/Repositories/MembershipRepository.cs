using Backtrack.Core.Application.Usecases.Organizations;
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
}
