using MediatR;

namespace Backtrack.Core.Application.Usecases.Subscriptions.CancelSubscription;

public sealed record CancelSubscriptionCommand : IRequest<SubscriptionResult>
{
    public required SubscriberContext Subscriber { get; init; }
    /// <summary>When true, cancels at period end instead of immediately.</summary>
    public bool CancelAtPeriodEnd { get; init; } = true;
}
