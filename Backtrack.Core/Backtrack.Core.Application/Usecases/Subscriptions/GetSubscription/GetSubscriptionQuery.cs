using MediatR;

namespace Backtrack.Core.Application.Usecases.Subscriptions.GetSubscription;

public sealed record GetSubscriptionQuery : IRequest<SubscriptionResult?>
{
    public required SubscriberContext Subscriber { get; init; }
}
