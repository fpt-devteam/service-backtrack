using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Interfaces.Repositories
{
    public interface IPostRepository : IGenericRepository<Post, Guid>
    {
        Task<(IEnumerable<Post> Items, int TotalCount)> SearchByFullTextAsync(
            string searchTerm,
            PagedQuery pagedQuery,
            PostFilters? filters = null,
            CancellationToken cancellationToken = default);

        Task<(IEnumerable<Post> Items, int TotalCount)> GetPagedAsync(
            PagedQuery pagedQuery,
            PostFilters? filters = null,
            CancellationToken cancellationToken = default);

        Task<(IEnumerable<(Post Post, double SimilarityScore, double? DistanceMeters)> Items, int TotalCount)> SearchBySemanticAsync(
            float[] queryEmbedding,
            PagedQuery pagedQuery,
            PostType? postType = null,
            GeoPoint? location = null,
            double? radiusInKm = null,
            Guid? organizationId = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<(Post Post, double Similarity, double DistanceMeters)>> GetSimilarPostsAsync(
            Post post,
            CancellationToken cancellationToken = default);

    }
}
