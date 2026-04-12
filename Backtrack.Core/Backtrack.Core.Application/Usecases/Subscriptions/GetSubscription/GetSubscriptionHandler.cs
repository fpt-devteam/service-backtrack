using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Subscriptions.GetSubscription;

public sealed class GetSubscriptionHandler(
    ISubscriptionRepository subscriptionRepository,
    ISubscriptionPlanRepository planRepository)
    : IRequestHandler<GetSubscriptionQuery, SubscriptionResult?>
{
    public async Task<SubscriptionResult?> Handle(GetSubscriptionQuery query, CancellationToken cancellationToken)
    {
        var subscriber = query.Subscriber;

        // Rule: orgId present → find by org; no orgId → find by user.
        Subscription? subscription = subscriber.SubscriberType switch
        {
            SubscriberType.Organization => await subscriptionRepository.GetActiveByOrganizationIdAsync(
                subscriber.OrganizationId!.Value, cancellationToken),
            SubscriberType.User => await subscriptionRepository.GetActiveByUserIdAsync(
                subscriber.UserId!, cancellationToken),
            _ => null
        };

        if (subscription is not null)
        {
            return MapToResult(subscription);
        }

        // If organization has no active subscription, return a virtual free tier
        if (subscriber.SubscriberType == SubscriberType.Organization)
        {
            var plans = await planRepository.GetActiveBySubscriberTypeAsync(SubscriberType.Organization, cancellationToken);
            var freePlan = plans.FirstOrDefault(p => p.Price == 0);

            if (freePlan != null)
            {
                return new SubscriptionResult
                {
                    Id = Guid.Empty,
                    SubscriberType = SubscriberType.Organization,
                    UserId = subscriber.UserId,
                    OrganizationId = subscriber.OrganizationId,
                    PlanId = freePlan.Id,
                    PlanSnapshot = new SubscriptionPlanSnapshot
                    {
                        Name = freePlan.Name,
                        Price = freePlan.Price,
                        Currency = freePlan.Currency,
                        BillingInterval = freePlan.BillingInterval,
                        Features = freePlan.Features,
                    },
                    Status = SubscriptionStatus.Active,
                    CurrentPeriodStart = DateTimeOffset.UtcNow,
                    CurrentPeriodEnd = DateTimeOffset.UtcNow.AddYears(100),
                    CancelAtPeriodEnd = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                };
            }
        }

        return null;
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
