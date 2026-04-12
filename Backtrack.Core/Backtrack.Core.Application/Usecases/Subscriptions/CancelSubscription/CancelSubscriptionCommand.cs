using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Subscriptions.CancelSubscription;

public sealed record CancelSubscriptionCommand : IRequest<SubscriptionResult>
{
    [JsonIgnore]
    public SubscriberContext? Subscriber { get; init; }
    public Guid? OrganizationId { get; init; }
    /// <summary>When true, cancels at period end instead of immediately.</summary>
    public bool CancelAtPeriodEnd { get; init; } = true;
}
