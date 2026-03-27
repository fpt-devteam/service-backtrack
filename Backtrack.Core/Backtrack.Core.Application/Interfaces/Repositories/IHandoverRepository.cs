using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IHandoverRepository : IGenericRepository<Handover, Guid>
{
    Task<Handover?> GetByIdWithExtensionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Handover?> GetByIdWithPostsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Handover>> GetExpiredPendingHandoversAsync(CancellationToken cancellationToken = default);
    Task<(List<Handover> Items, int Total)> GetByUserAsync(
        string userId,
        int page,
        int pageSize,
        HandoverStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveHandoverForPostsAsync(Guid? finderPostId, Guid? ownerPostId, CancellationToken cancellationToken = default);
}
