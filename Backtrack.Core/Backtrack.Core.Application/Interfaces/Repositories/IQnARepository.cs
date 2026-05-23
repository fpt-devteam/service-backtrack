using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for Question entities.
/// </summary>
public interface IQnARepository : IGenericRepository<Question, Guid>
{
    /// <summary>
    /// Returns a paginated list of questions for a post, ordered by creation date descending.
    /// </summary>
    Task CreateBatchAsync(IEnumerable<Question> questions, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Question> Items, int Total)> GetPagedByPostAsync(
        Guid postId,
        int offset,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single question by ID, eagerly loading its Answers collection.
    /// </summary>
    Task<Question?> GetByIdWithAnswersAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Question>> GetWithAnswersByPostAsync(
        Guid postId,
        string? answererId = null,
        CancellationToken cancellationToken = default);
}
