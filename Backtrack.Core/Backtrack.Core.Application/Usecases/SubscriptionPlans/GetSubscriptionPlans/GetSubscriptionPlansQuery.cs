using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.SubscriptionPlans.GetSubscriptionPlans;

public sealed record GetSubscriptionPlansQuery : IRequest<List<SubscriptionPlanResult>>
{
    public required SubscriberType SubscriberType { get; init; }
}
