using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public sealed class SubscriptionPlan : Entity<Guid>
{
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required string Currency { get; set; }
    public required SubscriptionBillingInterval BillingInterval { get; set; }
    public required SubscriberType SubscriberType { get; set; }
    public required string[] Features { get; set; }
    public required string ProviderPriceId { get; set; } // Stripe price ID
    public bool IsActive { get; set; } = true;
}
