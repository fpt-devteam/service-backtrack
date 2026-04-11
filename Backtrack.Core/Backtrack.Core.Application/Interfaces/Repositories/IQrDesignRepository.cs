using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IQrDesignRepository : IGenericRepository<QrDesign, Guid>
{
    Task<QrDesign?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
