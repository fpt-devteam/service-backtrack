using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IOrganizationRepository : IGenericRepository<Organization, Guid>
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<Organization?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<(List<Organization> Items, int Total)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get only required finder contact fields for an organization (efficient projection query)
    /// </summary>
    Task<List<OrgContractField>?> GetRequiredFinderContractFieldsByOrgIdAsync(Guid orgId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get only required owner form fields for an organization (efficient projection query)
    /// </summary>
    Task<List<OrgContractField>?> GetRequiredOwnerContractFieldsByOrgIdAsync(Guid orgId, CancellationToken cancellationToken = default);
}
