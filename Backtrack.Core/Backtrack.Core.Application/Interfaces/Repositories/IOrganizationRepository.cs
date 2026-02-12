using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IOrganizationRepository : IGenericRepository<Organization, Guid>
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
}
