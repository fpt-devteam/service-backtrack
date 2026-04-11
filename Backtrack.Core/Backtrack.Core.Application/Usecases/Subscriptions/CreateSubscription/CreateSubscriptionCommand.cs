using MediatR;

namespace Backtrack.Core.Application.Usecases.Subscriptions.CreateSubscription;

public sealed record CreateSubscriptionCommand : IRequest<SubscriptionResult>
{
    public required SubscriberContext Subscriber { get; init; }
    public required Guid PlanId { get; init; }
}
