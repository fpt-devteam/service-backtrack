using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.Admin.GetSubscriptionPlans;

public sealed record AdminSubscriptionPlanItem
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required string Currency { get; init; }
    public required SubscriptionBillingInterval BillingInterval { get; init; }
    public required string[] Features { get; init; }
    public required string ProviderPriceId { get; init; }
    public required bool IsActive { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed record AdminSubscriptionPlansResult
{
    public required List<AdminSubscriptionPlanItem> User { get; init; }
    public required List<AdminSubscriptionPlanItem> Organization { get; init; }
}
