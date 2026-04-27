using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.Admin.ArchiveSubscriptionPlan;

public sealed record ArchiveSubscriptionPlanResult
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; init; }
}
