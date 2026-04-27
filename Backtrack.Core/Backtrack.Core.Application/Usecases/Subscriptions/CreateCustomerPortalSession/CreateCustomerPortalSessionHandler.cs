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
        var membership = await membershipRepository.GetByOrgAndUserAsync(
            command.OrganizationId, command.CallerId!, cancellationToken)
            ?? throw new ForbiddenException(SubscriptionErrors.NotAdmin);

        if (membership.Role != MembershipRole.OrgAdmin)
            throw new ForbiddenException(SubscriptionErrors.NotAdmin);

        var subscription = await subscriptionRepository.GetActiveByOrganizationIdAsync(
            command.OrganizationId, cancellationToken)
            ?? throw new NotFoundException(SubscriptionErrors.NotFound);

        var url = await stripeService.CreateBillingPortalSessionAsync(
            subscription.ProviderCustomerId, command.ReturnUrl, cancellationToken);

        return new CreateCustomerPortalSessionResult { Url = url };
    }
}
