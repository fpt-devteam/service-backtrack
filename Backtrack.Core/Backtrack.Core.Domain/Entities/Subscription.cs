using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Domain.Entities;

public sealed class Subscription : Entity<Guid>
{
    public required SubscriberType SubscriberType { get; set; }
    public string? UserId { get; set; }
    public Guid? OrganizationId { get; set; }

    public required Guid PlanId { get; set; }
    public required SubscriptionPlanSnapshot PlanSnapshot { get; set; }

    public required string ProviderSubscriptionId { get; set; } // Stripe subscription ID
    public required string ProviderCustomerId { get; set; }     // Stripe customer ID

    public required SubscriptionStatus Status { get; set; }
    public required DateTimeOffset CurrentPeriodStart { get; set; }
    public required DateTimeOffset CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }

    public SubscriptionPlan Plan { get; set; } = default!;
    public Organization Organization { get; set; } = default!;
}
