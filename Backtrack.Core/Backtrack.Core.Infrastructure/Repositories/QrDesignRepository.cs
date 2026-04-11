using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class QrDesignRepository : CrudRepositoryBase<QrDesign, Guid>, IQrDesignRepository
{
    public QrDesignRepository(ApplicationDbContext context) : base(context) { }

    public async Task<QrDesign?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
}
