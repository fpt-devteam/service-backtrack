using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetRevenueTransactions;

public sealed class GetRevenueTransactionsHandler(
    IUserRepository           userRepository,
    IPaymentHistoryRepository paymentHistoryRepository)
    : IRequestHandler<GetRevenueTransactionsQuery, RevenueTransactionsPageResult>
{
    public async Task<RevenueTransactionsPageResult> Handle(
        GetRevenueTransactionsQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var (items, total) = await paymentHistoryRepository.GetPagedWithDetailsAsync(
            query.Page, query.PageSize,
            query.SubscriberType, query.Status, query.Search,
            cancellationToken);

        var userIds      = items.Where(p => p.UserId != null).Select(p => p.UserId!).Distinct();
        var displayNames = await userRepository.GetDisplayNamesByIdsAsync(userIds, cancellationToken);

        var results = items.Select(p =>
        {
            var isOrg       = p.SubscriberType == SubscriberType.Organization;
            var planName    = p.Subscription?.PlanSnapshot.Name;
            var orgName     = p.Subscription?.Organization?.Name;
            var orgId       = p.OrganizationId?.ToString();
            displayNames.TryGetValue(p.UserId ?? string.Empty, out var userName);

            return new RevenueTransactionResult
            {
                Id              = p.Id,
                RevenueType     = isOrg ? "Subscription" : "QR Sales",
                TenantId        = isOrg ? orgId : null,
                TenantName      = isOrg ? orgName : null,
                UserId          = !isOrg ? p.UserId : null,
                UserName        = !isOrg ? userName : null,
                Amount          = p.Amount,
                Currency        = p.Currency,
                Status          = MapStatus(p.Status),
                PaymentMethod   = "Stripe",
                TransactionDate = p.PaymentDate,
                Description     = isOrg
                    ? $"{planName} - Monthly Subscription"
                    : "QR Code Purchase",
                InvoiceNumber    = p.ProviderInvoiceId,
                SubscriptionPlan = isOrg ? planName : null,
            };
        }).ToList();

        return new RevenueTransactionsPageResult(results, total);
    }

    private static string MapStatus(PaymentStatus status) => status switch
    {
        PaymentStatus.Succeeded => "Completed",
        PaymentStatus.Failed    => "Failed",
        PaymentStatus.Pending   => "Pending",
        _                       => status.ToString()
    };
}
