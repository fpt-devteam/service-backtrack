using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.SubscriptionPlans.GetSubscriptionPlans;

public sealed class GetSubscriptionPlansHandler(ISubscriptionPlanRepository planRepository)
    : IRequestHandler<GetSubscriptionPlansQuery, List<SubscriptionPlanResult>>
{
    public async Task<List<SubscriptionPlanResult>> Handle(GetSubscriptionPlansQuery query, CancellationToken cancellationToken)
    {
        var plans = await planRepository.GetActiveBySubscriberTypeAsync(query.SubscriberType, cancellationToken);

        return plans.Select(p => new SubscriptionPlanResult
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Currency = p.Currency,
            BillingInterval = p.BillingInterval,
            SubscriberType = p.SubscriberType,
            Features = p.Features,
        }).ToList();
    }
}
