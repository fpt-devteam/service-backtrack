using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories
{
    public class PostMatchRepository(ApplicationDbContext context) : CrudRepositoryBase<PostMatch, Guid>(context), IPostMatchRepository
    {
        public async Task CreateRangeAsync(IEnumerable<PostMatch> postMatches, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var match in postMatches)
            {
                match.CreatedAt = now;
            }
            await _dbSet.AddRangeAsync(postMatches, cancellationToken);
        }

        public async Task DeleteByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            await _dbSet
                .Where(pm => (pm.LostPostId == postId || pm.FoundPostId == postId) && pm.DeletedAt == null)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(pm => pm.DeletedAt, now)
                    .SetProperty(pm => pm.UpdatedAt, now),
                    cancellationToken);
        }

        public async Task<int> CountMatchedPostsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT COUNT(DISTINCT id)::int FROM (
                    SELECT lost_post_id  AS id FROM post_matches WHERE deleted_at IS NULL
                    UNION
                    SELECT found_post_id AS id FROM post_matches WHERE deleted_at IS NULL
                ) matched_ids";

            var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await _context.Database.OpenConnectionAsync(cancellationToken);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
        }

        public async Task<IEnumerable<PostMatch>> GetMatchesByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(pm => pm.LostPost)
                .Include(pm => pm.FoundPost)
                .Where(pm => (pm.LostPostId == postId || pm.FoundPostId == postId) && pm.DeletedAt == null)
                .OrderByDescending(pm => pm.Score)
                .ToListAsync(cancellationToken);
        }
    }
}
