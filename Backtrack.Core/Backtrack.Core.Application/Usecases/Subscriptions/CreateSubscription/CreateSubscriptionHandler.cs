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
    IUserRepository userRepository,
    IMembershipRepository membershipRepository,
    IOrganizationRepository organizationRepository,
    IStripeService stripeService)
    : IRequestHandler<CreateSubscriptionCommand, CreateSubscriptionResult>
{
    public async Task<CreateSubscriptionResult> Handle(CreateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var subscriber = command.Subscriber!;

        // Resolve email — for org, also verify caller is OrgAdmin
        string email;
        if (subscriber.SubscriberType == SubscriberType.User)
        {
            var user = await userRepository.GetByIdAsync(subscriber.UserId!)
                ?? throw new NotFoundException(UserErrors.NotFound);
            email = user.Email ?? throw new NotFoundException(UserErrors.NotFound);
        }
        else
        {
            var membership = await membershipRepository.GetByOrgAndUserAsync(subscriber.OrganizationId!.Value, subscriber.UserId!, cancellationToken)
                ?? throw new ForbiddenException(SubscriptionErrors.NotAdmin);
            if (membership.Role != MembershipRole.OrgAdmin)
                throw new ForbiddenException(SubscriptionErrors.NotAdmin);

            var adminUser = await userRepository.GetByIdAsync(subscriber.UserId!)
                ?? throw new NotFoundException(UserErrors.NotFound);
            email = adminUser.Email ?? throw new NotFoundException(UserErrors.NotFound);
        }

        // Reuse existing incomplete subscription for the same plan
        Subscription? incomplete = subscriber.SubscriberType switch
        {
            SubscriberType.User => await subscriptionRepository.GetIncompleteByUserIdAsync(subscriber.UserId!, command.PlanId, cancellationToken),
            SubscriberType.Organization => await subscriptionRepository.GetIncompleteByOrganizationIdAsync(subscriber.OrganizationId!.Value, command.PlanId, cancellationToken),
            _ => null
        };

        if (incomplete is not null)
        {
            var existingClientSecret = await stripeService.GetClientSecretAsync(incomplete.ProviderSubscriptionId, cancellationToken);
            return MapToResult(incomplete, existingClientSecret);
        }

        // Block if already active
        bool hasActive = subscriber.SubscriberType switch
        {
            SubscriberType.User => await subscriptionRepository.GetActiveByUserIdAsync(subscriber.UserId!, cancellationToken) is not null,
            SubscriberType.Organization => await subscriptionRepository.GetActiveByOrganizationIdAsync(subscriber.OrganizationId!.Value, cancellationToken) is not null,
            _ => false
        };
        if (hasActive) throw new ConflictException(SubscriptionErrors.AlreadyActive);

        var plan = await planRepository.GetByIdAsync(command.PlanId)
            ?? throw new NotFoundException(SubscriptionErrors.PlanNotFound);

        if (plan.SubscriberType != subscriber.SubscriberType)
            throw new ForbiddenException(SubscriptionErrors.Forbidden);

        // Ensure Stripe customer
        var externalId = subscriber.SubscriberType == SubscriberType.User
            ? subscriber.UserId!
            : subscriber.OrganizationId!.Value.ToString();

        var customerId = await stripeService.EnsureCustomerAsync(new EnsureCustomerRequest
        {
            ExternalId = externalId,
            Email = email,
        }, cancellationToken);

        var stripeResult = await stripeService.CreateSubscriptionAsync(new CreateSubscriptionRequest
        {
            CustomerId = customerId,
            PriceId = plan.ProviderPriceId,
        }, cancellationToken);

        var subscription = await subscriptionRepository.CreateAsync(new Subscription
        {
            Id = Guid.NewGuid(),
            SubscriberType = subscriber.SubscriberType,
            UserId = subscriber.UserId,
            OrganizationId = subscriber.OrganizationId,
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
        return MapToResult(subscription, stripeResult.ClientSecret);
    }

    private static CreateSubscriptionResult MapToResult(Subscription s, string? clientSecret) => new()
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
        ClientSecret = clientSecret ?? string.Empty,
    };
}
