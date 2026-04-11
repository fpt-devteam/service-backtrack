using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IPaymentHistoryRepository : IGenericRepository<PaymentHistory, Guid>
{
    Task<List<PaymentHistory>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
