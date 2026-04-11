using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Subscriptions.GetSubscription;

public sealed class GetSubscriptionHandler(ISubscriptionRepository subscriptionRepository)
    : IRequestHandler<GetSubscriptionQuery, SubscriptionResult?>
{
    public async Task<SubscriptionResult?> Handle(GetSubscriptionQuery query, CancellationToken cancellationToken)
    {
        Subscription? subscription = query.Subscriber.SubscriberType switch
        {
            SubscriberType.User => await subscriptionRepository.GetActiveByUserIdAsync(query.Subscriber.UserId!, cancellationToken),
            SubscriberType.Organization => await subscriptionRepository.GetActiveByOrganizationIdAsync(query.Subscriber.OrganizationId!.Value, cancellationToken),
            _ => null
        };

        if (subscription is null) return null;

        return MapToResult(subscription);
    }

    internal static SubscriptionResult MapToResult(Subscription s) => new()
    {
        Id = s.Id,
        SubscriberType = s.SubscriberType,
        UserId = s.UserId,
        OrganizationId = s.OrganizationId,
        PlanId = s.PlanId,
        PlanSnapshot = s.PlanSnapshot,
        Status = s.Status,
        CurrentPeriodStart = s.CurrentPeriodStart,
        CurrentPeriodEnd = s.CurrentPeriodEnd,
        CancelAtPeriodEnd = s.CancelAtPeriodEnd,
        CreatedAt = s.CreatedAt,
    };
}
