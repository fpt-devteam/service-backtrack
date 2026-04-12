using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Subscriptions.GetSubscription;
using Backtrack.Core.Application.Usecases.Users;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetUserDetail;

public sealed class GetUserDetailHandler(
    IUserRepository userRepository,
    ISubscriptionRepository subscriptionRepository,
    IQrCodeRepository qrCodeRepository,
    IPaymentHistoryRepository paymentHistoryRepository) : IRequestHandler<GetUserDetailQuery, AdminUserDetailResult>
{
    public async Task<AdminUserDetailResult> Handle(GetUserDetailQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var user = await userRepository.GetByIdAsync(query.TargetUserId)
            ?? throw new NotFoundException(UserErrors.NotFound);

        var subscription    = await subscriptionRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        var qrCount         = await qrCodeRepository.CountByUserIdAsync(user.Id, cancellationToken);
        var (payments, _)   = await paymentHistoryRepository.GetPagedByUserIdAsync(
            user.Id, query.BillingPage, query.BillingPageSize, cancellationToken);

        return new AdminUserDetailResult
        {
            BasicInfo    = user.ToUserResult(),
            Subscription = subscription is null ? null : GetSubscriptionHandler.MapToResult(subscription),
            QrUsage      = new QrUsageOverview { TotalQrCodes = qrCount },
            BillingHistory = payments.Select(p => new PaymentHistoryResult
            {
                Id                = p.Id,
                Amount            = p.Amount,
                Currency          = p.Currency,
                Status            = p.Status,
                PaymentDate       = p.PaymentDate,
                ProviderInvoiceId = p.ProviderInvoiceId,
                PlanName          = p.Subscription?.PlanSnapshot?.Name
            }).ToList()
        };
    }
}
