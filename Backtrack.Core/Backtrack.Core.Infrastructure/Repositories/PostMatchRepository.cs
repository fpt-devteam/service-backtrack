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

        public async Task DeleteByFoundPostIdsAsync(IEnumerable<Guid> foundPostIds, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            await _dbSet
                .Where(pm => foundPostIds.Contains(pm.FoundPostId) && pm.DeletedAt == null)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(pm => pm.DeletedAt, now)
                    .SetProperty(pm => pm.UpdatedAt, now),
                    cancellationToken);
        }

        public async Task DeleteByLostPostIdsAsync(IEnumerable<Guid> lostPostIds, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            await _dbSet
                .Where(pm => lostPostIds.Contains(pm.LostPostId) && pm.DeletedAt == null)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(pm => pm.DeletedAt, now)
                    .SetProperty(pm => pm.UpdatedAt, now),
                    cancellationToken);
        }

        public async Task<IEnumerable<PostMatch>> GetMatchesByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
        {
            var post = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == postId && p.DeletedAt == null, cancellationToken);
            if (post == null) return Array.Empty<PostMatch>();

            var query = _dbSet.AsNoTracking();

            if (post.PostType == Backtrack.Core.Domain.Constants.PostType.Lost)
            {
                return await query
                    .Include(pm => pm.FoundPost)
                    .Where(pm => pm.LostPostId == postId && pm.DeletedAt == null)
                    .OrderByDescending(pm => pm.MatchScore)
                    .ToListAsync(cancellationToken);
            }
            else
            {
                return await query
                    .Include(pm => pm.LostPost)
                    .Where(pm => pm.FoundPostId == postId && pm.DeletedAt == null)
                    .OrderByDescending(pm => pm.MatchScore)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
