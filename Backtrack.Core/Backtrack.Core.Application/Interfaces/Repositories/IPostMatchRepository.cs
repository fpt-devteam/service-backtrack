using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories
{
    public interface IPostMatchRepository : IGenericRepository<PostMatch, Guid>
    {
        Task CreateRangeAsync(IEnumerable<PostMatch> postMatches, CancellationToken cancellationToken = default);
        Task DeleteBySourcePostIdsAsync(IEnumerable<Guid> sourcePostIds, CancellationToken cancellationToken = default);
        Task DeleteByCandidatePostIdsAsync(IEnumerable<Guid> candidatePostIds, CancellationToken cancellationToken = default);
        Task<IEnumerable<PostMatch>> GetMatchesByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
        Task<int> CountMatchedPostsAsync(CancellationToken cancellationToken = default);
    }
}
