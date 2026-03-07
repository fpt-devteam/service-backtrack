using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories
{
    public interface IPostMatchRepository : IGenericRepository<PostMatch, Guid>
    {
        Task CreateRangeAsync(IEnumerable<PostMatch> postMatches, CancellationToken cancellationToken = default);
        Task DeleteByFoundPostIdsAsync(IEnumerable<Guid> foundPostIds, CancellationToken cancellationToken = default);
        Task DeleteByLostPostIdsAsync(IEnumerable<Guid> lostPostIds, CancellationToken cancellationToken = default);
    }
}
