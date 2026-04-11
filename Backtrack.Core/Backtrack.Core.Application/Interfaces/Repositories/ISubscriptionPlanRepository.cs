using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface ISubscriptionPlanRepository : IGenericRepository<SubscriptionPlan, Guid>
{
    Task<List<SubscriptionPlan>> GetActiveBySubscriberTypeAsync(SubscriberType subscriberType, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetByProviderPriceIdAsync(string providerPriceId, CancellationToken cancellationToken = default);
}
