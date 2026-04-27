using System.Text.Json;
using Backtrack.Core.Application.Interfaces.Payments;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Backtrack.Core.Infrastructure.Payments;

public sealed class StripeService : IStripeService
{
    private readonly StripeSettings _settings;
    private readonly CustomerService _customerService;
    private readonly SubscriptionService _subscriptionService;
    private readonly Stripe.BillingPortal.SessionService _billingPortalSessionService;
    private readonly ILogger<StripeService> _logger;

    public StripeService(IOptions<StripeSettings> options, ILogger<StripeService> logger)
    {
        _settings = options.Value;
        _logger = logger;
        StripeConfiguration.ApiKey = _settings.SecretKey;
        _customerService = new CustomerService();
        _subscriptionService = new SubscriptionService();
        _billingPortalSessionService = new Stripe.BillingPortal.SessionService();
    }

    public async Task<string> EnsureCustomerAsync(EnsureCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var searchOptions = new CustomerSearchOptions
        {
            Query = $"metadata['external_id']:'{request.ExternalId}'",
            Limit = 1
        };

        var existing = await _customerService.SearchAsync(searchOptions, cancellationToken: cancellationToken);

        if (existing.Data.Count > 0)
            return existing.Data[0].Id;

        var created = await _customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = request.Email,
            Metadata = new Dictionary<string, string> { { "external_id", request.ExternalId } }
        }, cancellationToken: cancellationToken);

        return created.Id;
    }

    public async Task<CreateSubscriptionResult> CreateSubscriptionAsync(
    CreateSubscriptionRequest request, CancellationToken cancellationToken = default)
{
    var options = new SubscriptionCreateOptions
    {
        Customer = request.CustomerId,
        Items = [new SubscriptionItemOptions { Price = request.PriceId }],
        PaymentBehavior = "default_incomplete",
    };

    options.AddExpand("latest_invoice.payment_intent");

    var subscription = await _subscriptionService.CreateAsync(options, cancellationToken: cancellationToken);

    return new CreateSubscriptionResult
    {
        SubscriptionId = subscription.Id,
        ClientSecret = subscription.LatestInvoice?.PaymentIntent?.ClientSecret,
        Status = MapStripeStatus(subscription.Status), // Trạng thái lúc này sẽ là "Incomplete"
        CurrentPeriodStart = subscription.CurrentPeriodStart,
        CurrentPeriodEnd = subscription.CurrentPeriodEnd,
    };
}

    public async Task<string?> GetClientSecretAsync(string providerSubscriptionId, CancellationToken cancellationToken = default)
    {
        var options = new SubscriptionGetOptions();
        options.AddExpand("latest_invoice.payment_intent");
        var subscription = await _subscriptionService.GetAsync(providerSubscriptionId, options, cancellationToken: cancellationToken);
        return subscription.LatestInvoice?.PaymentIntent?.ClientSecret;
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
        var stripeEvent = EventUtility.ConstructEvent(json, signature, _settings.WebhookSecret, throwOnApiVersionMismatch: false);
        return Task.FromResult(ParseStripeEvent(stripeEvent, json));
    }

    private StripeWebhookEvent ParseStripeEvent(Event stripeEvent, string rawJson)
    {

        string? providerSubscriptionId = null;
        string? providerInvoiceId = null;
        SubscriptionStatus? newStatus = null;
        DateTimeOffset? periodStart = null;
        DateTimeOffset? periodEnd = null;
        bool? cancelAtPeriodEnd = null;
        long? invoiceAmountPaid = null;
        string? invoiceCurrency = null;
        string? invoiceUrl = null;

        _logger.LogInformation("Stripe webhook received: Type={Type}, DataObjectType={ObjType}",
            stripeEvent.Type, stripeEvent.Data.Object?.GetType().Name ?? "null");

        if (stripeEvent.Data.Object is Stripe.Subscription sub)
        {
            providerSubscriptionId = sub.Id;
            newStatus = MapStripeStatus(sub.Status);
            cancelAtPeriodEnd = sub.CancelAtPeriodEnd;

            // SDK v47 may deserialize Unix timestamps as epoch (1970-01-01) when the API
            // version mismatches. Fall back to raw JSON when the parsed value is epoch.
            var epochThreshold = new DateTime(1970, 1, 2, 0, 0, 0, DateTimeKind.Utc);
            periodStart = sub.CurrentPeriodStart > epochThreshold
                ? (DateTimeOffset)sub.CurrentPeriodStart
                : GetDateTimeOffsetFromRawJson(rawJson, "data", "object", "current_period_start");
            periodEnd = sub.CurrentPeriodEnd > epochThreshold
                ? (DateTimeOffset)sub.CurrentPeriodEnd
                : GetDateTimeOffsetFromRawJson(rawJson, "data", "object", "current_period_end");

            _logger.LogInformation("Subscription parsed: Type={Type}, SubId={SubId}, Status={Status}, PeriodStart={Start}, PeriodEnd={End}",
                stripeEvent.Type, sub.Id, sub.Status, periodStart, periodEnd);
        }
        else if (stripeEvent.Data.Object is Invoice invoice)
        {
            // In Stripe API 2025-05-28.basil, Invoice.subscription was removed.
            // It now lives at invoice.parent.subscription_details.subscription.
            // SDK v47 deserializes SubscriptionId as null — parse from raw JSON.
            providerSubscriptionId = invoice.SubscriptionId
                ?? GetIdFromRawJson(rawJson, "data", "object", "parent", "subscription_details", "subscription");
            providerInvoiceId = invoice.Id;
            invoiceAmountPaid = invoice.AmountPaid;
            invoiceCurrency = invoice.Currency;
            invoiceUrl = invoice.HostedInvoiceUrl
                ?? GetStringFromRawJson(rawJson, "data", "object", "hosted_invoice_url");

            _logger.LogInformation("Invoice parsed: Type={Type}, InvoiceId={InvoiceId}, SubscriptionId={SubId}, AmountPaid={Amount}",
                stripeEvent.Type, invoice.Id, providerSubscriptionId, invoice.AmountPaid);
        }
        else if (stripeEvent.Type is "invoice_payment.paid" or "invoice_payment.failed")
        {
            // New event in Stripe API 2025-05-28.basil — SDK v47 has no InvoicePayment type.
            (providerSubscriptionId, providerInvoiceId, invoiceAmountPaid, invoiceCurrency) =
                ParseInvoicePaymentFromRawJson(rawJson);
            // invoice_payment object holds only an invoice ID, not the full invoice — URL not available here.

            _logger.LogInformation("InvoicePayment parsed: Type={Type}, InvoiceId={InvoiceId}, SubscriptionId={SubId}, AmountPaid={Amount}",
                stripeEvent.Type, providerInvoiceId, providerSubscriptionId, invoiceAmountPaid);
        }
        else
        {
            _logger.LogInformation("Stripe webhook ignored: Type={Type}, RawType={ObjType}",
                stripeEvent.Type, stripeEvent.Data.Object?.GetType().FullName ?? "null");
        }

        return new StripeWebhookEvent
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
            InvoiceUrl = invoiceUrl,
            EventCreatedAt = stripeEvent.Created,
        };
    }

    /// <summary>
    /// Parses an invoice_payment.paid / invoice_payment.failed event from raw JSON.
    /// SDK v47 has no InvoicePayment type — Data.Object is null for these events.
    /// Expected shape: data.object.{ subscription, invoice, amount_paid, currency }
    /// </summary>
    private (string? subscriptionId, string? invoiceId, long? amountPaid, string? currency) ParseInvoicePaymentFromRawJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var obj = doc.RootElement.GetProperty("data").GetProperty("object");

            var subscriptionId = ResolveIdField(obj, "subscription");
            var invoiceId      = ResolveIdField(obj, "invoice");
            long? amountPaid   = obj.TryGetProperty("amount_paid", out var amEl) && amEl.ValueKind == JsonValueKind.Number
                                     ? amEl.GetInt64() : null;
            string? currency   = obj.TryGetProperty("currency", out var curEl) && curEl.ValueKind == JsonValueKind.String
                                     ? curEl.GetString() : null;

            return (subscriptionId, invoiceId, amountPaid, currency);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse invoice_payment from raw JSON");
            return (null, null, null, null);
        }
    }

    /// <summary>
    /// Walks a path in raw JSON and returns the string value (not necessarily an ID).
    /// </summary>
    private static string? GetStringFromRawJson(string json, params string[] path)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            JsonElement current = doc.RootElement;
            foreach (var key in path)
            {
                if (!current.TryGetProperty(key, out current))
                    return null;
            }
            return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
        }
        catch { return null; }
    }

    /// <summary>
    /// Walks a path in raw JSON, reads a Unix timestamp (integer), and converts to DateTimeOffset.
    /// </summary>
    private static DateTimeOffset? GetDateTimeOffsetFromRawJson(string json, params string[] path)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            JsonElement current = doc.RootElement;
            foreach (var key in path)
            {
                if (!current.TryGetProperty(key, out current))
                    return null;
            }
            if (current.ValueKind == JsonValueKind.Number && current.TryGetInt64(out var seconds) && seconds > 0)
                return DateTimeOffset.FromUnixTimeSeconds(seconds);
            return null;
        }
        catch { return null; }
    }

    /// <summary>
    /// Walks a dot-separated path in raw JSON and returns the string ID at that location.
    /// Handles both plain string values and expandable objects ({ "id": "xxx" }).
    /// </summary>
    private string? GetIdFromRawJson(string json, params string[] path)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            JsonElement current = doc.RootElement;
            foreach (var key in path)
            {
                if (!current.TryGetProperty(key, out current))
                    return null;
            }
            return ResolveIdField(current);
        }
        catch { return null; }
    }

    /// <summary>Extracts a string ID from a field that is either a plain string or an expandable object.</summary>
    private static string? ResolveIdField(JsonElement el)
        => el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Object => el.TryGetProperty("id", out var idEl) ? idEl.GetString() : null,
            _ => null
        };

    private static string? ResolveIdField(JsonElement parent, string field)
    {
        if (!parent.TryGetProperty(field, out var el)) return null;
        return ResolveIdField(el);
    }

    public async Task<string> CreateBillingPortalSessionAsync(
        string customerId, string returnUrl, CancellationToken cancellationToken = default)
    {
        var session = await _billingPortalSessionService.CreateAsync(
            new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customerId,
                ReturnUrl = returnUrl,
            },
            cancellationToken: cancellationToken);

        return session.Url;
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
