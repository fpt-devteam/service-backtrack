using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class OrgFormTemplateRepository : CrudRepositoryBase<OrgFormTemplate, Guid>, IOrgFormTemplateRepository
{
    public OrgFormTemplateRepository(ApplicationDbContext context) : base(context) { }

    public async Task<OrgFormTemplate?> GetByOrgIdAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.OrgId == orgId, cancellationToken);
    }
}
