using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface ISubscriptionRepository : IGenericRepository<Subscription, Guid>
{
    Task<Subscription?> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetActiveByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetIncompleteByUserIdAsync(string userId, Guid planId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetIncompleteByOrganizationIdAsync(Guid organizationId, Guid planId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByProviderSubscriptionIdAsync(string providerSubscriptionId, CancellationToken cancellationToken = default);

    Task<(int UserCount, int OrgCount)> GetActiveCountsAsync(CancellationToken cancellationToken = default);

    Task<decimal> GetMrrAsync(CancellationToken cancellationToken = default);
}
