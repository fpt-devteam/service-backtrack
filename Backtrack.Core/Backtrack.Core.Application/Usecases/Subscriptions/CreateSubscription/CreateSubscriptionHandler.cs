using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Payments;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Subscriptions.CreateSubscription;

public sealed class CreateSubscriptionHandler(
    ISubscriptionRepository subscriptionRepository,
    ISubscriptionPlanRepository planRepository,
    IStripeService stripeService)
    : IRequestHandler<CreateSubscriptionCommand, SubscriptionResult>
{
    public async Task<SubscriptionResult> Handle(CreateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        // Check no active subscription already
        bool hasActive = command.Subscriber.SubscriberType switch
        {
            SubscriberType.User => await subscriptionRepository.GetActiveByUserIdAsync(command.Subscriber.UserId!, cancellationToken) is not null,
            SubscriberType.Organization => await subscriptionRepository.GetActiveByOrganizationIdAsync(command.Subscriber.OrganizationId!.Value, cancellationToken) is not null,
            _ => false
        };
        if (hasActive) throw new ConflictException(SubscriptionErrors.AlreadyActive);

        var plan = await planRepository.GetByIdAsync(command.PlanId)
            ?? throw new NotFoundException(SubscriptionErrors.PlanNotFound);

        if (plan.SubscriberType != command.Subscriber.SubscriberType)
            throw new ForbiddenException(SubscriptionErrors.Forbidden);

        // Ensure Stripe customer
        var externalId = command.Subscriber.SubscriberType == SubscriberType.User
            ? command.Subscriber.UserId!
            : command.Subscriber.OrganizationId!.Value.ToString();

        var customerId = await stripeService.EnsureCustomerAsync(new EnsureCustomerRequest
        {
            ExternalId = externalId,
            Email = command.Subscriber.Email,
            Name = command.Subscriber.Name,
        }, cancellationToken);

        var stripeResult = await stripeService.CreateSubscriptionAsync(new CreateSubscriptionRequest
        {
            CustomerId = customerId,
            PriceId = plan.ProviderPriceId,
        }, cancellationToken);

        var subscription = await subscriptionRepository.CreateAsync(new Subscription
        {
            Id = Guid.NewGuid(),
            SubscriberType = command.Subscriber.SubscriberType,
            UserId = command.Subscriber.UserId,
            OrganizationId = command.Subscriber.OrganizationId,
            PlanId = plan.Id,
            PlanSnapshot = new SubscriptionPlanSnapshot
            {
                Name = plan.Name,
                Price = plan.Price,
                Currency = plan.Currency,
                BillingInterval = plan.BillingInterval,
                Features = plan.Features,
            },
            ProviderSubscriptionId = stripeResult.SubscriptionId,
            ProviderCustomerId = customerId,
            Status = stripeResult.Status,
            CurrentPeriodStart = stripeResult.CurrentPeriodStart,
            CurrentPeriodEnd = stripeResult.CurrentPeriodEnd,
            CancelAtPeriodEnd = false,
        });

        await subscriptionRepository.SaveChangesAsync();
        return GetSubscription.GetSubscriptionHandler.MapToResult(subscription);
    }
}
