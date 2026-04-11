using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
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

    public async Task<Subscription?> GetByProviderSubscriptionIdAsync(string providerSubscriptionId, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(s => s.ProviderSubscriptionId == providerSubscriptionId, cancellationToken);
}
