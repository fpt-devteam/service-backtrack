using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories
{
    public interface IOrganizationInventoryRepository : IGenericRepository<OrganizationInventory, Guid>
    {
        Task<(IEnumerable<OrganizationInventory> Items, int TotalCount)> GetPagedAsync(
            int offset,
            int limit,
            Guid? orgId = null,
            string? loggedById = null,
            OrganizationInventoryStatus? status = null,
            string? searchTerm = null,
            CancellationToken cancellationToken = default);

        Task<(IEnumerable<(OrganizationInventory Inventory, double SimilarityScore)> Items, int TotalCount)> SearchBySemanticAsync(
            float[] queryEmbedding,
            int offset,
            int limit,
            Guid? orgId = null,
            CancellationToken cancellationToken = default);
    }
}
