using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IPostImageRepository : IGenericRepository<PostImage, Guid>
{
    Task<IEnumerable<PostImage>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task DeleteByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
}
