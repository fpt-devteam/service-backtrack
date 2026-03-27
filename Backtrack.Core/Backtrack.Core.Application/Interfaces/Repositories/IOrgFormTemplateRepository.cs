using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IOrgFormTemplateRepository : IGenericRepository<OrgFormTemplate, Guid>
{
    Task<OrgFormTemplate?> GetByOrgIdAsync(Guid orgId, CancellationToken cancellationToken = default);
}
