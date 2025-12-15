using Backtrack.Core.Application.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using Backtrack.Core.Infrastructure.Data;
using Backtrack.Core.Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Backtrack.Core.Infrastructure.Repositories
{
    public class PostRepository(ApplicationDbContext context) : CrudRepositoryBase<Post, Guid>(context), IPostRepository
    {
        public async Task<(IEnumerable<Post> Items, int TotalCount)> GetPagedAsync(
            int offset,
            int limit,
            string? postType = null,
            string? searchTerm = null,
            double? latitude = null,
            double? longitude = null,
            double? radiusInKm = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(postType) && Enum.TryParse<PostType>(postType, out var postTypeEnum))
            {
                query = query.Where(p => p.PostType == postTypeEnum);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.ItemName.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));
            }

            if (latitude.HasValue && longitude.HasValue && radiusInKm.HasValue)
            {
                var searchLocation = new GeoPoint(latitude.Value, longitude.Value);

                var radiusInMeters = radiusInKm.Value * 1000;

                var ids = await _context.Database
                    .SqlQueryRaw<Guid>(@"
                        SELECT id
                        FROM posts
                        WHERE location IS NOT NULL
                        AND ST_DWithin(
                            location::geography,
                            ST_SetSRID(ST_MakePoint({0}, {1}), 4326)::geography,
                            {2}
                        )
                        AND deleted_at IS NULL
                    ", longitude.Value, latitude.Value, radiusInMeters)
                    .ToListAsync(cancellationToken);

                query = query.Where(p => ids.Contains(p.Id));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(p => p.EventTime)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
