using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.Admin.UpdateSubscriptionPlanFeatures;

public sealed record UpdateSubscriptionPlanFeaturesResult
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string[] Features { get; init; }
}
