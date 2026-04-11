using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Payments;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Subscriptions.CancelSubscription;

public sealed class CancelSubscriptionHandler(
    ISubscriptionRepository subscriptionRepository,
    IStripeService stripeService)
    : IRequestHandler<CancelSubscriptionCommand, SubscriptionResult>
{
    public async Task<SubscriptionResult> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken)
    {
        Subscription? subscription = command.Subscriber.SubscriberType switch
        {
            SubscriberType.User => await subscriptionRepository.GetActiveByUserIdAsync(command.Subscriber.UserId!, cancellationToken),
            SubscriberType.Organization => await subscriptionRepository.GetActiveByOrganizationIdAsync(command.Subscriber.OrganizationId!.Value, cancellationToken),
            _ => null
        };

        if (subscription is null) throw new NotFoundException(SubscriptionErrors.NotFound);

        await stripeService.CancelSubscriptionAsync(
            subscription.ProviderSubscriptionId,
            command.CancelAtPeriodEnd,
            cancellationToken);

        if (!command.CancelAtPeriodEnd)
            subscription.Status = SubscriptionStatus.Canceled;

        subscription.CancelAtPeriodEnd = command.CancelAtPeriodEnd;
        subscriptionRepository.Update(subscription);
        await subscriptionRepository.SaveChangesAsync();

        return GetSubscription.GetSubscriptionHandler.MapToResult(subscription);
    }
}
