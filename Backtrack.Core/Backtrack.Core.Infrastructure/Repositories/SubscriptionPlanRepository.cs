using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class SubscriptionPlanRepository : CrudRepositoryBase<SubscriptionPlan, Guid>, ISubscriptionPlanRepository
{
    public SubscriptionPlanRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<SubscriptionPlan>> GetActiveBySubscriberTypeAsync(
        SubscriberType subscriberType, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Where(p => p.IsActive && p.SubscriberType == subscriberType)
            .OrderBy(p => p.Price)
            .ToListAsync(cancellationToken);

    public async Task<SubscriptionPlan?> GetByProviderPriceIdAsync(
        string providerPriceId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProviderPriceId == providerPriceId, cancellationToken);
}
