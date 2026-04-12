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
        var subscriber = query.Subscriber;

        // Rule: orgId present → find by org; no orgId → find by user.
        // Never mix: an org subscription is NOT the caller's personal subscription.
        Subscription? subscription = subscriber.SubscriberType switch
        {
            SubscriberType.Organization => await subscriptionRepository.GetActiveByOrganizationIdAsync(
                subscriber.OrganizationId!.Value, cancellationToken),
            SubscriberType.User => await subscriptionRepository.GetActiveByUserIdAsync(
                subscriber.UserId!, cancellationToken),
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
