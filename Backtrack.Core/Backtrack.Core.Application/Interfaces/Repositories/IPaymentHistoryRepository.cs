using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IPaymentHistoryRepository : IGenericRepository<PaymentHistory, Guid>
{
    Task<List<PaymentHistory>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

    Task<(List<PaymentHistory> Items, int Total)> GetPagedByUserIdAsync(
        string userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<(List<PaymentHistory> Items, int Total)> GetPagedByOrgIdAsync(
        Guid orgId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<(decimal Total, decimal ThisMonth, decimal LastMonth, decimal UserRevenue, decimal OrgRevenue)>
        GetRevenueSummaryAsync(CancellationToken cancellationToken = default);

    Task<List<(string Period, decimal Amount, int TransactionCount)>> GetRevenueChartAsync(
        int months, CancellationToken cancellationToken = default);
}
