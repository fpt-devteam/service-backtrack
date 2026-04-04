using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class OrganizationRepository : CrudRepositoryBase<Organization, Guid>, IOrganizationRepository
{
    public OrganizationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(o => o.Slug == slug, cancellationToken);
    }

    public async Task<(List<Organization> Items, int Total)> GetAllAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().OrderByDescending(o => o.CreatedAt);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<List<FinderContactField>?> GetRequiredFinderContactFieldsByOrgIdAsync(
        Guid orgId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.Id == orgId)
            .Select(o => o.RequiredFinderContactFields)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<FinderContactField>?> GetRequiredOwnerFormFieldsByOrgIdAsync(
        Guid orgId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.Id == orgId)
            .Select(o => o.RequiredOwnerFormFields)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
