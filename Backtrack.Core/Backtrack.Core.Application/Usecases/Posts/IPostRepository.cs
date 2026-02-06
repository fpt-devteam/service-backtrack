using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Usecases.Posts
{
    public interface IPostRepository : IGenericRepository<Post, Guid>
    {
        Task<(IEnumerable<Post> Items, int TotalCount)> GetPagedAsync(
            int offset,
            int limit,
            PostType? postType = null,
            string? searchTerm = null,
            double? latitude = null,
            double? longitude = null,
            double? radiusInKm = null,
            string? authorId = null,
            CancellationToken cancellationToken = default);

        Task<(IEnumerable<(Post Post, double SimilarityScore)> Items, int TotalCount)> SearchBySemanticAsync(
            float[] queryEmbedding,
            int offset,
            int limit,
            PostType? postType = null,
            double? latitude = null,
            double? longitude = null,
            double? radiusInKm = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<(Post Post, double SimilarityScore)>> GetSimilarPostsAsync(
            Guid postId,
            PostType postType,
            float[] embedding,
            double? latitude,
            double? longitude,
            double radiusInKm,
            int limit = 20,
            CancellationToken cancellationToken = default);
    }
}
