using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Usecases.Organizations;

public interface IOrganizationRepository : IGenericRepository<Organization, Guid>
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
}
