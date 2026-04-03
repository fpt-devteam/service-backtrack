using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Interfaces.Repositories
{
    public interface IPostRepository : IGenericRepository<Post, Guid>
    {
        Task<IEnumerable<Post>> SearchByFullTextAsync(
            string searchTerm,
            PostFilters? filters = null,
            CancellationToken cancellationToken = default);

        Task<(IEnumerable<(Post Post, double? DistanceMeters)> Items, int TotalCount)> GetPagedAsync(
            PagedQuery pagedQuery,
            string? searchTerm = null,
            PostType? postType = null,
            Guid? organizationId = null,
            GeoPoint? location = null,
            double? radiusInKm = null,
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

        Task<(IEnumerable<(Post Post, double DistanceMeters)> Items, int TotalCount)> GetFeedAsync(
            GeoPoint location,
            int offset,
            int limit,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Post>> GetByAuthorIdAsync(string authorId, CancellationToken cancellationToken = default);
    }
}
