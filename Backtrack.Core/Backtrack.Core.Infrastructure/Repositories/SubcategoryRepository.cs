using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public sealed class SubcategoryRepository(ApplicationDbContext context) : ISubcategoryRepository
{
    public Task<List<Subcategory>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        => context.Set<Subcategory>()
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ToListAsync(cancellationToken);

    public Task<Subcategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Set<Subcategory>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Subcategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => context.Set<Subcategory>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Code == code, cancellationToken);
}
