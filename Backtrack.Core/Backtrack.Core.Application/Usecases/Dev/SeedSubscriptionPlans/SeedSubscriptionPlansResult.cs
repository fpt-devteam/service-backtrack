namespace Backtrack.Core.Application.Usecases.Dev.SeedSubscriptionPlans;

public sealed record SeedSubscriptionPlansResult
{
    public required int Created { get; init; }
    public required int Skipped { get; init; }
    public required List<string> CreatedPlanNames { get; init; }
}
