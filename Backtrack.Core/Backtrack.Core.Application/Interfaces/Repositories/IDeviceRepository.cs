using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IDeviceRepository
{
    Task UpsertAsync(Device device, CancellationToken ct = default);
    Task<bool> DeactivateAsync(string userId, string deviceId, CancellationToken ct = default);
    Task<IReadOnlyList<Device>> GetActiveByUserIdAsync(string userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
