using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for QnA entities.
/// </summary>
public interface IQnARepository : IGenericRepository<QnA, Guid>
{
    /// <summary>
    /// Returns a paginated list of all non-deleted questions ordered by creation date descending.
    /// </summary>
    Task<(IReadOnlyList<QnA> Items, int Total)> GetPagedByPostAsync(
        Guid postId,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);
}
