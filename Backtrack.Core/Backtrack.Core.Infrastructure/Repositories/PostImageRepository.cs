using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class PostImageRepository(ApplicationDbContext context)
    : CrudRepositoryBase<PostImage, Guid>(context), IPostImageRepository
{
    public async Task<IEnumerable<PostImage>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(i => i.PostId == postId && i.DeletedAt == null)
            .OrderBy(i => i.DisplayOrder)
            .ThenBy(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var images = await _dbSet
            .Where(i => i.PostId == postId && i.DeletedAt == null)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var image in images)
        {
            image.DeletedAt = now;
            image.UpdatedAt = now;
        }
    }
}
