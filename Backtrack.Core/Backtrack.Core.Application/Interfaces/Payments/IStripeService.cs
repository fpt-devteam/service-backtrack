using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Interfaces.Payments;

public sealed record CreateSubscriptionRequest
{
    public required string CustomerId { get; init; }
    public required string PriceId { get; init; }
}

public sealed record CreateSubscriptionResult
{
    public required string SubscriptionId { get; init; }
    public required SubscriptionStatus Status { get; init; }
    public required DateTimeOffset CurrentPeriodStart { get; init; }
    public required DateTimeOffset CurrentPeriodEnd { get; init; }
    public required string ClientSecret { get; init; }
}

public sealed record EnsureCustomerRequest
{
    public required string ExternalId { get; init; } // userId or orgId
    public required string Email { get; init; }
}

/// <summary>Domain-level representation of a parsed Stripe webhook event.</summary>
public sealed record StripeWebhookEvent
{
    public required string Type { get; init; }
    public required string? ProviderSubscriptionId { get; init; }
    public required string? ProviderInvoiceId { get; init; }
    public required SubscriptionStatus? NewSubscriptionStatus { get; init; }
    public required DateTimeOffset? CurrentPeriodStart { get; init; }
    public required DateTimeOffset? CurrentPeriodEnd { get; init; }
    public required bool? CancelAtPeriodEnd { get; init; }
    // Invoice fields
    public required long? InvoiceAmountPaid { get; init; } // cents
    public required string? InvoiceCurrency { get; init; }
    public required string? InvoiceUrl { get; init; }
    public required DateTimeOffset EventCreatedAt { get; init; }
}

public interface IStripeService
{
    Task<string> EnsureCustomerAsync(EnsureCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CreateSubscriptionResult> CreateSubscriptionAsync(CreateSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<string?> GetClientSecretAsync(string providerSubscriptionId, CancellationToken cancellationToken = default);
    Task CancelSubscriptionAsync(string providerSubscriptionId, bool cancelAtPeriodEnd, CancellationToken cancellationToken = default);
    Task<StripeWebhookEvent> ParseWebhookEventAsync(string json, string signature);
}
