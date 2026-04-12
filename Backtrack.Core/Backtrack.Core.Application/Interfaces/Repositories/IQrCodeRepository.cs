using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IQrCodeRepository : IGenericRepository<QrCode, Guid>
{
    Task<QrCode?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<QrCode?> GetByPublicCodeAsync(string publicCode, CancellationToken cancellationToken = default);
    Task<bool> PublicCodeExistsAsync(string publicCode, CancellationToken cancellationToken = default);

    Task<int> CountByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
