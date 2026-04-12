using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtrack.Core.Application.Usecases.Subscriptions.CreateSubscription;

public sealed record CreateSubscriptionResult
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
    public required string ClientSecret { get; init; }
}
