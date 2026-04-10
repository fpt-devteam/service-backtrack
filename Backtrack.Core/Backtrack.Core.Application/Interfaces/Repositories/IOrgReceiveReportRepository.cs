using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IOrgReceiveReportRepository : IGenericRepository<OrgReceiveReport, Guid>
{
    Task<OrgReceiveReport?> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, OrgReceiveReport>> GetByPostIdsAsync(IEnumerable<Guid> postIds, CancellationToken cancellationToken = default);
}
