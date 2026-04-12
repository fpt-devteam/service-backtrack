using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Payments;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.StripeWebhooks.HandleStripeEvent;

public sealed class HandleStripeEventHandler(
    IStripeService stripeService,
    ISubscriptionRepository subscriptionRepository,
    IPaymentHistoryRepository paymentHistoryRepository,
    ILogger<HandleStripeEventHandler> logger)
    : IRequestHandler<HandleStripeEventCommand>
{
    public async Task<Unit> Handle(HandleStripeEventCommand command, CancellationToken cancellationToken)
    {
        StripeWebhookEvent webhookEvent;
        try
        {
            webhookEvent = await stripeService.ParseWebhookEventAsync(command.Json, command.Signature);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Stripe webhook validation failed");
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
            case "invoice_payment.paid":   // Stripe API 2025-05-28.basil
            case "invoice_payment.failed":
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
        if (ev.ProviderSubscriptionId is null || ev.ProviderInvoiceId is null)
        {
            logger.LogWarning("HandleInvoice skipped: ProviderSubscriptionId={SubId}, ProviderInvoiceId={InvId}",
                ev.ProviderSubscriptionId, ev.ProviderInvoiceId);
            return;
        }

        var subscription = await subscriptionRepository.GetByProviderSubscriptionIdAsync(ev.ProviderSubscriptionId, cancellationToken);
        if (subscription is null)
        {
            logger.LogWarning("HandleInvoice skipped: no subscription found for ProviderSubscriptionId={SubId}", ev.ProviderSubscriptionId);
            return;
        }

        var status = ev.Type is "invoice.payment_succeeded" or "invoice_payment.paid"
            ? PaymentStatus.Succeeded
            : PaymentStatus.Failed;

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
            InvoiceUrl = ev.InvoiceUrl,
        });

        await paymentHistoryRepository.SaveChangesAsync();
        logger.LogInformation("Payment record created: InvoiceId={InvoiceId}, Status={Status}, Amount={Amount}",
            ev.ProviderInvoiceId, status, (ev.InvoiceAmountPaid ?? 0) / 100m);
    }
}
