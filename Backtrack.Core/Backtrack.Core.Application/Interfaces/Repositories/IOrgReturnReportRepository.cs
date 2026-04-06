using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IOrgReturnReportRepository : IGenericRepository<OrgReturnReport, Guid>
{
    Task<OrgReturnReport?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveForPostAsync(Guid postId, CancellationToken cancellationToken = default);
}
