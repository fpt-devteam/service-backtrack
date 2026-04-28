using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueSummary;

public sealed class GetRevenueSummaryHandler(
    IUserRepository           userRepository,
    IPaymentHistoryRepository paymentHistoryRepository)
    : IRequestHandler<GetRevenueSummaryQuery, RevenueSummaryResult>
{
    public async Task<RevenueSummaryResult> Handle(
        GetRevenueSummaryQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var (total, thisMonth, lastMonth, userRevenue, orgRevenue) =
            await paymentHistoryRepository.GetRevenueSummaryAsync(cancellationToken);

        var (totalCount, orgCount, userCount) =
            await paymentHistoryRepository.GetTransactionCountsAsync(cancellationToken);

        var growth = lastMonth > 0
            ? Math.Round((thisMonth - lastMonth) / lastMonth * 100, 1)
            : 0m;

        var avg = totalCount > 0 ? Math.Round(total / totalCount, 2) : 0m;

        return new RevenueSummaryResult
        {
            TotalRevenue             = total,
            MonthlyRevenue           = thisMonth,
            GrowthPercentage         = growth,
            TotalTransactions        = totalCount,
            AverageTransactionValue  = avg,
            SubscriptionRevenue      = orgRevenue,
            QrSalesRevenue           = userRevenue,
            SubscriptionTransactions = orgCount,
            QrSalesTransactions      = userCount,
        };
    }
}
