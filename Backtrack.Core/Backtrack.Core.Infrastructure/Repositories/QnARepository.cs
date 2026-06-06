using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

/// <summary>
/// EF Core repository implementation for Question entities.
/// </summary>
public sealed class QnARepository(ApplicationDbContext context)
    : CrudRepositoryBase<Question, Guid>(context), IQnARepository
{
    public async Task CreateBatchAsync(IEnumerable<Question> questions, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(questions, cancellationToken);
    }

    public async Task<(IReadOnlyList<Question> Items, int Total)> GetPagedByPostAsync(
        Guid postId,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(q => q.PostId == postId)
            .OrderByDescending(q => q.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(offset).Take(limit).ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IReadOnlyList<Question>> GetWithAnswersByPostAsync(
        Guid postId,
        string? answererId = null,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(q => q.PostId == postId)
            .Include(q => q.Answers.Where(a =>
                a.DeletedAt == null &&
                (answererId == null || a.AnswererId == answererId)))
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Question?> GetByIdWithAnswersAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id && q.DeletedAt == null, cancellationToken);
    }
}
