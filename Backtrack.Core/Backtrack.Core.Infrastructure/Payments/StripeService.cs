using Backtrack.Core.Application.Interfaces.Payments;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Stripe;

namespace Backtrack.Core.Infrastructure.Payments;

public sealed class StripeService : IStripeService
{
    private readonly StripeSettings _settings;
    private readonly CustomerService _customerService;
    private readonly SubscriptionService _subscriptionService;

    public StripeService(IOptions<StripeSettings> options)
    {
        _settings = options.Value;
        StripeConfiguration.ApiKey = _settings.SecretKey;
        _customerService = new CustomerService();
        _subscriptionService = new SubscriptionService();
    }

    public async Task<string> EnsureCustomerAsync(EnsureCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var listOptions = new CustomerListOptions { Limit = 1 };
        listOptions.AddExtraParam("metadata[external_id]", request.ExternalId);

        var existing = await _customerService.ListAsync(listOptions, cancellationToken: cancellationToken);
        if (existing.Data.Count > 0)
            return existing.Data[0].Id;

        var created = await _customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = request.Email,
            Name = request.Name,
            Metadata = new Dictionary<string, string> { ["external_id"] = request.ExternalId },
        }, cancellationToken: cancellationToken);

        return created.Id;
    }

    public async Task<CreateSubscriptionResult> CreateSubscriptionAsync(
        CreateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionService.CreateAsync(new SubscriptionCreateOptions
        {
            Customer = request.CustomerId,
            Items = [new SubscriptionItemOptions { Price = request.PriceId }],
        }, cancellationToken: cancellationToken);

        return new CreateSubscriptionResult
        {
            SubscriptionId = subscription.Id,
            Status = MapStripeStatus(subscription.Status),
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
        };
    }

    public async Task CancelSubscriptionAsync(
        string providerSubscriptionId, bool cancelAtPeriodEnd, CancellationToken cancellationToken = default)
    {
        if (cancelAtPeriodEnd)
        {
            await _subscriptionService.UpdateAsync(providerSubscriptionId,
                new SubscriptionUpdateOptions { CancelAtPeriodEnd = true },
                cancellationToken: cancellationToken);
        }
        else
        {
            await _subscriptionService.CancelAsync(providerSubscriptionId,
                cancellationToken: cancellationToken);
        }
    }

    public Task<StripeWebhookEvent> ParseWebhookEventAsync(string json, string signature)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, signature, _settings.WebhookSecret);

        string? providerSubscriptionId = null;
        string? providerInvoiceId = null;
        SubscriptionStatus? newStatus = null;
        DateTimeOffset? periodStart = null;
        DateTimeOffset? periodEnd = null;
        bool? cancelAtPeriodEnd = null;
        long? invoiceAmountPaid = null;
        string? invoiceCurrency = null;

        if (stripeEvent.Data.Object is Stripe.Subscription sub)
        {
            providerSubscriptionId = sub.Id;
            newStatus = MapStripeStatus(sub.Status);
            periodStart = sub.CurrentPeriodStart;
            periodEnd = sub.CurrentPeriodEnd;
            cancelAtPeriodEnd = sub.CancelAtPeriodEnd;
        }
        else if (stripeEvent.Data.Object is Invoice invoice)
        {
            providerSubscriptionId = invoice.SubscriptionId;
            providerInvoiceId = invoice.Id;
            invoiceAmountPaid = invoice.AmountPaid;
            invoiceCurrency = invoice.Currency;
        }

        return Task.FromResult(new StripeWebhookEvent
        {
            Type = stripeEvent.Type,
            ProviderSubscriptionId = providerSubscriptionId,
            ProviderInvoiceId = providerInvoiceId,
            NewSubscriptionStatus = newStatus,
            CurrentPeriodStart = periodStart,
            CurrentPeriodEnd = periodEnd,
            CancelAtPeriodEnd = cancelAtPeriodEnd,
            InvoiceAmountPaid = invoiceAmountPaid,
            InvoiceCurrency = invoiceCurrency,
            EventCreatedAt = stripeEvent.Created,
        });
    }

    private static SubscriptionStatus MapStripeStatus(string status) => status switch
    {
        "active" => SubscriptionStatus.Active,
        "past_due" => SubscriptionStatus.PastDue,
        "unpaid" => SubscriptionStatus.Unpaid,
        "incomplete" => SubscriptionStatus.Incomplete,
        "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
        "canceled" => SubscriptionStatus.Canceled,
        _ => SubscriptionStatus.Incomplete,
    };
}
