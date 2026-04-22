using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IC2CReturnReportRepository : IGenericRepository<C2CReturnReport, Guid>
{
    Task<C2CReturnReport?> GetByIdWithExtensionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<C2CReturnReport?> GetByIdWithPostsAsync(Guid id, CancellationToken cancellationToken = default);
    // Task<List<C2CReturnReport>> GetExpiredPendingReturnReportsAsync(CancellationToken cancellationToken = default);
    Task<(List<C2CReturnReport> Items, int Total)> GetByUserAsync(
        string userId,
        int page,
        int pageSize,
        C2CReturnReportStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<List<C2CReturnReport>> GetByPartnerAsync(
        string userId,
        string partnerId,
        CancellationToken cancellationToken = default);
    Task<C2CReturnReport?> GetOngoingByParticipantsAndPostsAsync(
        string finderId,
        string ownerId,
        Guid finderPostId,
        Guid ownerPostId,
        CancellationToken cancellationToken = default);
}