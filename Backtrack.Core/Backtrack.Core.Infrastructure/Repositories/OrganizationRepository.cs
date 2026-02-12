using Backtrack.Core.Application.Interfaces.Repositories;
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
}
