using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public sealed class DeviceRepository : IDeviceRepository
{
    private readonly ApplicationDbContext _context;

    public DeviceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task UpsertAsync(Device device, CancellationToken ct = default)
    {
        var existing = await _context.Devices
            .FirstOrDefaultAsync(d => d.UserId == device.UserId && d.DeviceId == device.DeviceId, ct);

        if (existing is not null)
        {
            existing.Token = device.Token;
            existing.IsActive = true;
            existing.LastSeenAt = DateTimeOffset.UtcNow;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            _context.Devices.Add(device);
        }
    }

    public async Task<bool> DeactivateAsync(string userId, string deviceId, CancellationToken ct = default)
    {
        var existing = await _context.Devices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceId, ct);

        if (existing is null)
            return false;

        existing.IsActive = false;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }

    public async Task<IReadOnlyList<Device>> GetActiveByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _context.Devices
            .Where(d => d.UserId == userId && d.IsActive)
            .ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
