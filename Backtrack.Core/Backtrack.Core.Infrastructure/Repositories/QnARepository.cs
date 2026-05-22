using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

/// <summary>
/// EF Core repository implementation for QnA entities.
/// </summary>
public sealed class QnARepository(ApplicationDbContext context)
    : CrudRepositoryBase<QnA, Guid>(context), IQnARepository
{
    public async Task<(IReadOnlyList<QnA> Items, int Total)> GetPagedAsync(
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .OrderByDescending(q => q.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(offset).Take(limit).ToListAsync(cancellationToken);

        return (items, total);
    }
}
