using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Usecases.Organizations;

public interface IMembershipRepository : IGenericRepository<Membership, Guid>
{
    Task<Membership?> GetByOrgAndUserAsync(Guid orgId, string userId, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Membership> Items, int TotalCount)> GetPagedByOrgAsync(
        Guid orgId, int offset, int limit, CancellationToken cancellationToken = default);

    Task<IEnumerable<Membership>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<int> CountActiveAdminsAsync(Guid orgId, CancellationToken cancellationToken = default);
}
