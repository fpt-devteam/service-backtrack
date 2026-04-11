using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Payments;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.StripeWebhooks.HandleStripeEvent;

public sealed class HandleStripeEventHandler(
    IStripeService stripeService,
    ISubscriptionRepository subscriptionRepository,
    IPaymentHistoryRepository paymentHistoryRepository)
    : IRequestHandler<HandleStripeEventCommand>
{
    public async Task<Unit> Handle(HandleStripeEventCommand command, CancellationToken cancellationToken)
    {
        StripeWebhookEvent webhookEvent;
        try
        {
            webhookEvent = await stripeService.ParseWebhookEventAsync(command.Json, command.Signature);
        }
        catch
        {
            throw new UnauthorizedException(SubscriptionErrors.WebhookSignatureInvalid);
        }

        switch (webhookEvent.Type)
        {
            case "customer.subscription.updated":
            case "customer.subscription.deleted":
                await HandleSubscriptionChangedAsync(webhookEvent, cancellationToken);
                break;

            case "invoice.payment_succeeded":
            case "invoice.payment_failed":
                await HandleInvoiceAsync(webhookEvent, cancellationToken);
                break;
        }

        return Unit.Value;
    }

    private async Task HandleSubscriptionChangedAsync(StripeWebhookEvent ev, CancellationToken cancellationToken)
    {
        if (ev.ProviderSubscriptionId is null) return;

        var subscription = await subscriptionRepository.GetByProviderSubscriptionIdAsync(ev.ProviderSubscriptionId, cancellationToken);
        if (subscription is null) return;

        if (ev.NewSubscriptionStatus.HasValue)
            subscription.Status = ev.NewSubscriptionStatus.Value;
        if (ev.CurrentPeriodStart.HasValue)
            subscription.CurrentPeriodStart = ev.CurrentPeriodStart.Value;
        if (ev.CurrentPeriodEnd.HasValue)
            subscription.CurrentPeriodEnd = ev.CurrentPeriodEnd.Value;
        if (ev.CancelAtPeriodEnd.HasValue)
            subscription.CancelAtPeriodEnd = ev.CancelAtPeriodEnd.Value;

        subscriptionRepository.Update(subscription);
        await subscriptionRepository.SaveChangesAsync();
    }

    private async Task HandleInvoiceAsync(StripeWebhookEvent ev, CancellationToken cancellationToken)
    {
        if (ev.ProviderSubscriptionId is null || ev.ProviderInvoiceId is null) return;

        var subscription = await subscriptionRepository.GetByProviderSubscriptionIdAsync(ev.ProviderSubscriptionId, cancellationToken);
        if (subscription is null) return;

        var status = ev.Type == "invoice.payment_succeeded" ? PaymentStatus.Succeeded : PaymentStatus.Failed;

        await paymentHistoryRepository.CreateAsync(new PaymentHistory
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            SubscriberType = subscription.SubscriberType,
            UserId = subscription.UserId,
            OrganizationId = subscription.OrganizationId,
            ProviderInvoiceId = ev.ProviderInvoiceId,
            Amount = (ev.InvoiceAmountPaid ?? 0) / 100m,
            Currency = ev.InvoiceCurrency ?? "usd",
            Status = status,
            PaymentDate = ev.EventCreatedAt,
        });

        await paymentHistoryRepository.SaveChangesAsync();
    }
}
