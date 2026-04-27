using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories
{
    public interface IPostRepository : IGenericRepository<Post, Guid>
    {
        Task<IEnumerable<Post>> SearchByTitleAsync(
            string title,
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

        Task<IEnumerable<(Post Post, double SimilarityScore)>> GetSimilarPostsAsync(
            Post post,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Post>> GetCardMatchCandidatesAsync(
            Post post,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            PostFilters? filters = null,
            CancellationToken cancellationToken = default);

        Task<Dictionary<(PostType Type, PostStatus Status), int>> GetBreakdownAsync(
            CancellationToken cancellationToken = default);

        Task<List<(string Period, int Count)>> GetGrowthChartAsync(
            int months,
            CancellationToken cancellationToken = default);

        Task<List<(string Period, PostType PostType, PostStatus Status, int Count)>> GetMonthlyPostKpiAsync(
            int months,
            CancellationToken cancellationToken = default);

        Task<List<(int Year, int Month, PostType PostType, int Count)>> GetPostMonthlyAsync(
            int months,
            CancellationToken cancellationToken = default);

        Task<Dictionary<Guid, (int Total, int Returned)>> GetStatsByOrgIdsAsync(
            IEnumerable<Guid> orgIds,
            CancellationToken cancellationToken = default);

        Task<HashSet<DateOnly>> GetActiveDatesAsync(
            string authorId,
            Guid orgId,
            DateTimeOffset since,
            CancellationToken cancellationToken = default);

        Task<Dictionary<(PostType Type, string EffectiveStatus), int>> GetStatusBreakdownByOrgAsync(
            Guid orgId,
            string? authorId,
            CancellationToken cancellationToken = default);
    }
}
