using MediatR;

namespace Backtrack.Core.Application.Usecases.Dev.SeedSubscriptionPlans;

/// <summary>
/// Seeds default subscription plans. Each plan's ProviderPriceId must be
/// the corresponding Stripe Price ID created in your Stripe dashboard.
/// </summary>
public sealed record SeedSubscriptionPlansCommand : IRequest<SeedSubscriptionPlansResult>
{
    // User QR plans
    public required string UserMonthlyPriceId { get; init; }
    public required string UserYearlyPriceId { get; init; }

    // Organization plans
    public required string OrgFreePriceId { get; init; }
    public required string OrgProPriceId { get; init; }
    public required string OrgMaxPriceId { get; init; }
}
