using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories;

public class SubscriptionRepository : CrudRepositoryBase<Subscription, Guid>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Subscription?> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active, cancellationToken);

    public async Task<Subscription?> GetActiveByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId && s.Status == SubscriptionStatus.Active, cancellationToken);

    public async Task<Subscription?> GetIncompleteByUserIdAsync(string userId, Guid planId, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(s => s.UserId == userId && s.PlanId == planId && s.Status == SubscriptionStatus.Incomplete, cancellationToken);

    public async Task<Subscription?> GetIncompleteByOrganizationIdAsync(Guid organizationId, Guid planId, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId && s.PlanId == planId && s.Status == SubscriptionStatus.Incomplete, cancellationToken);

    public async Task<Subscription?> GetByProviderSubscriptionIdAsync(string providerSubscriptionId, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(s => s.ProviderSubscriptionId == providerSubscriptionId, cancellationToken);

    public async Task<(int UserCount, int OrgCount)> GetActiveCountsAsync(CancellationToken cancellationToken = default)
    {
        var counts = await _dbSet
            .Where(s => s.Status == SubscriptionStatus.Active)
            .GroupBy(s => s.SubscriberType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var userCount = counts.FirstOrDefault(c => c.Type == SubscriberType.User)?.Count ?? 0;
        var orgCount  = counts.FirstOrDefault(c => c.Type == SubscriberType.Organization)?.Count ?? 0;
        return (userCount, orgCount);
    }

    public async Task<List<Subscription>> GetByOrgIdsAsync(
        IEnumerable<Guid> orgIds,
        CancellationToken cancellationToken = default)
    {
        var ids = orgIds.ToList();
        return await _dbSet.AsNoTracking()
            .Where(s => s.OrganizationId.HasValue && ids.Contains(s.OrganizationId!.Value))
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetMrrAsync(CancellationToken cancellationToken = default)
    {
        var snapshots = await _dbSet
            .AsNoTracking()
            .Where(s => s.Status == SubscriptionStatus.Active)
            .Select(s => new { s.PlanSnapshot.Price, s.PlanSnapshot.BillingInterval })
            .ToListAsync(cancellationToken);

        return snapshots.Sum(s =>
            s.BillingInterval == SubscriptionBillingInterval.Monthly
                ? s.Price
                : s.Price / 12m);
    }
}
