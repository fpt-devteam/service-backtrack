using Backtrack.Core.Application.Common.Interfaces;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Posts
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
            CancellationToken cancellationToken = default);
    }
}
