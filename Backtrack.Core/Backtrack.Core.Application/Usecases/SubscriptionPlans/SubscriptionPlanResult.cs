using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.SubscriptionPlans;

public sealed record SubscriptionPlanResult
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required string Currency { get; init; }
    public required SubscriptionBillingInterval BillingInterval { get; init; }
    public required SubscriberType SubscriberType { get; init; }
    public required string[] Features { get; init; }
}
