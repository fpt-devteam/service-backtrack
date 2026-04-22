using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IOrganizationRepository : IGenericRepository<Organization, Guid>
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<Organization?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<(List<Organization> Items, int Total)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<(List<Organization> Items, int Total)> GetPagedAdminAsync(
        int page,
        int pageSize,
        string? search = null,
        OrganizationStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<(int Total, int Active, int NewThisMonth)> GetCountsAsync(CancellationToken cancellationToken = default);

    Task<List<(string Period, int Count)>> GetGrowthChartAsync(int months, CancellationToken cancellationToken = default);

    Task<List<Organization>> GetAllForAdminAsync(string? search = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get only required finder contact fields for an organization (efficient projection query)
    /// </summary>
    Task<List<OrgContractField>?> GetRequiredFinderContractFieldsByOrgIdAsync(Guid orgId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get only required owner form fields for an organization (efficient projection query)
    /// </summary>
    Task<List<OrgContractField>?> GetRequiredOwnerContractFieldsByOrgIdAsync(Guid orgId, CancellationToken cancellationToken = default);
}
