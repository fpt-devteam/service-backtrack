using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface ISubcategoryRepository
{
    Task<List<Subcategory>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<Subcategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Subcategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
