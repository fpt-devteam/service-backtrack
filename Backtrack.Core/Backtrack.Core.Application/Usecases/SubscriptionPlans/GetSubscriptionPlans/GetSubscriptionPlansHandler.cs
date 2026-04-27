using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.SubscriptionPlans.GetSubscriptionPlans;

public sealed class GetSubscriptionPlansHandler(ISubscriptionPlanRepository planRepository)
    : IRequestHandler<GetSubscriptionPlansQuery, List<SubscriptionPlanResult>>
{
    internal static readonly SubscriptionPlanResult OrgFreePlan = new()
    {
        Id = Guid.Empty,
        Name = "Free",
        Price = 0,
        Currency = "USD",
        BillingInterval = SubscriptionBillingInterval.Monthly,
        SubscriberType = SubscriberType.Organization,
        Features = ["Up to 3 staff members", "Basic lost & found management", "Public organization profile"],
    };

    public async Task<List<SubscriptionPlanResult>> Handle(GetSubscriptionPlansQuery query, CancellationToken cancellationToken)
    {
        var plans = await planRepository.GetActiveBySubscriberTypeAsync(query.SubscriberType, cancellationToken);

        var result = plans.Select(p => new SubscriptionPlanResult
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Currency = p.Currency,
            BillingInterval = p.BillingInterval,
            SubscriberType = p.SubscriberType,
            Features = p.Features,
        }).ToList();

        if (query.SubscriberType == SubscriberType.Organization)
            result.Insert(0, OrgFreePlan);

        return result;
    }
}
