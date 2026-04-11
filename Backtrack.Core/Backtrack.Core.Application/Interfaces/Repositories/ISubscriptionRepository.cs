using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface ISubscriptionRepository : IGenericRepository<Subscription, Guid>
{
    Task<Subscription?> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetActiveByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByProviderSubscriptionIdAsync(string providerSubscriptionId, CancellationToken cancellationToken = default);
}
