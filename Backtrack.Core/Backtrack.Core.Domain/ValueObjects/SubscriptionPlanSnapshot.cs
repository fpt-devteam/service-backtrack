using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.ValueObjects;

public sealed record SubscriptionPlanSnapshot
{
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required string Currency { get; init; }
    public required SubscriptionBillingInterval BillingInterval { get; init; }
    public required string[] Features { get; init; }
}
