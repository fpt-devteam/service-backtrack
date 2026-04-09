using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories
{
    public interface IPostRepository : IGenericRepository<Post, Guid>
    {
        Task<IEnumerable<Post>> SearchByFullTextAsync(
            string searchTerm,
            PostFilters? filters = null,
            CancellationToken cancellationToken = default);

        Task<(IEnumerable<Post> Items, int TotalCount)> GetPagedAsync(
            PagedQuery pagedQuery,
            PostFilters? filters = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<(Post Post, double SimilarityScore)>> SearchBySemanticAsync(
            float[] queryEmbedding,
            PostFilters? filters = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<(Post Post, double Similarity, double DistanceMeters)>> GetSimilarPostsAsync(
            Post post,
            CancellationToken cancellationToken = default);

    }
}
