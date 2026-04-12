using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Subscriptions.CreateSubscription;

public sealed record CreateSubscriptionCommand : IRequest<CreateSubscriptionResult>
{
    [JsonIgnore]
    public SubscriberContext? Subscriber { get; init; }
    public Guid? OrganizationId { get; init; }
    public required Guid PlanId { get; init; }
}
