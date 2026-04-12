using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Subscriptions.GetPaymentHistories;

public sealed class GetPaymentHistoriesHandler(
    IPaymentHistoryRepository paymentHistoryRepository,
    IMembershipRepository membershipRepository)
    : IRequestHandler<GetPaymentHistoriesQuery, PagedResult<PaymentHistoryResult>>
{
    public async Task<PagedResult<PaymentHistoryResult>> Handle(GetPaymentHistoriesQuery query, CancellationToken cancellationToken)
    {
        var subscriber = query.Subscriber;

        List<PaymentHistory> items;
        int total;

        if (subscriber.SubscriberType == SubscriberType.Organization)
        {
            // Verify caller is OrgAdmin before exposing org payment history
            var membership = await membershipRepository.GetByOrgAndUserAsync(
                subscriber.OrganizationId!.Value, subscriber.UserId!, cancellationToken)
                ?? throw new ForbiddenException(SubscriptionErrors.NotAdmin);

            if (membership.Role != MembershipRole.OrgAdmin)
                throw new ForbiddenException(SubscriptionErrors.NotAdmin);

            (items, total) = await paymentHistoryRepository.GetPagedByOrgIdAsync(
                subscriber.OrganizationId!.Value, query.Page, query.PageSize, cancellationToken);
        }
        else
        {
            (items, total) = await paymentHistoryRepository.GetPagedByUserIdAsync(
                subscriber.UserId!, query.Page, query.PageSize, cancellationToken);
        }

        return new PagedResult<PaymentHistoryResult>(total, items.Select(MapToResult).ToList());
    }

    private static PaymentHistoryResult MapToResult(PaymentHistory p) => new()
    {
        Id = p.Id,
        SubscriptionId = p.SubscriptionId,
        SubscriberType = p.SubscriberType,
        UserId = p.UserId,
        OrganizationId = p.OrganizationId,
        ProviderInvoiceId = p.ProviderInvoiceId,
        Amount = p.Amount,
        Currency = p.Currency,
        Status = p.Status,
        PaymentDate = p.PaymentDate,
        InvoiceUrl = p.InvoiceUrl,
        CreatedAt = p.CreatedAt,
    };
}
