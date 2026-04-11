using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.Subscriptions;

public sealed record SubscriptionResult
{
    public required Guid Id { get; init; }
    public required SubscriberType SubscriberType { get; init; }
    public string? UserId { get; init; }
    public Guid? OrganizationId { get; init; }
    public required Guid PlanId { get; init; }
    public required SubscriptionPlanSnapshot PlanSnapshot { get; init; }
    public required SubscriptionStatus Status { get; init; }
    public required DateTimeOffset CurrentPeriodStart { get; init; }
    public required DateTimeOffset CurrentPeriodEnd { get; init; }
    public required bool CancelAtPeriodEnd { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
