using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class PaymentHistoryRepository : CrudRepositoryBase<PaymentHistory, Guid>, IPaymentHistoryRepository
{
    public PaymentHistoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<PaymentHistory>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
}
