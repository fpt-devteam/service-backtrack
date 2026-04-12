using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class QrCodeRepository : CrudRepositoryBase<QrCode, Guid>, IQrCodeRepository
{
    public QrCodeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<QrCode?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(q => q.UserId == userId, cancellationToken);

    public async Task<QrCode?> GetByPublicCodeAsync(string publicCode, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(q => q.PublicCode == publicCode, cancellationToken);

    public async Task<bool> PublicCodeExistsAsync(string publicCode, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(q => q.PublicCode == publicCode, cancellationToken);

    public async Task<int> CountByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(q => q.UserId == userId, cancellationToken);
}
