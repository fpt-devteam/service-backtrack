using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Payments;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Subscriptions.CreateCustomerPortalSession;

public sealed class CreateCustomerPortalSessionHandler(
    ISubscriptionRepository subscriptionRepository,
    IMembershipRepository membershipRepository,
    IStripeService stripeService)
    : IRequestHandler<CreateCustomerPortalSessionCommand, CreateCustomerPortalSessionResult>
{
    public async Task<CreateCustomerPortalSessionResult> Handle(
        CreateCustomerPortalSessionCommand command, CancellationToken cancellationToken)
    {
        var subscriber = command.Subscriber;
        string providerCustomerId;

        if (subscriber.SubscriberType == SubscriberType.Organization)
        {
            var membership = await membershipRepository.GetByOrgAndUserAsync(
                subscriber.OrganizationId!.Value, command.CallerId!, cancellationToken)
                ?? throw new ForbiddenException(SubscriptionErrors.NotAdmin);

            if (membership.Role != MembershipRole.OrgAdmin)
                throw new ForbiddenException(SubscriptionErrors.NotAdmin);

            var subscription = await subscriptionRepository.GetActiveByOrganizationIdAsync(
                subscriber.OrganizationId!.Value, cancellationToken)
                ?? throw new NotFoundException(SubscriptionErrors.NotFound);

            providerCustomerId = subscription.ProviderCustomerId;
        }
        else
        {
            var subscription = await subscriptionRepository.GetActiveByUserIdAsync(
                command.CallerId!, cancellationToken)
                ?? throw new NotFoundException(SubscriptionErrors.NotFound);

            providerCustomerId = subscription.ProviderCustomerId;
        }

        var url = await stripeService.CreateBillingPortalSessionAsync(
            providerCustomerId, command.ReturnUrl, cancellationToken);

        return new CreateCustomerPortalSessionResult { Url = url };
    }
}
