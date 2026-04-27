using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Admin.ArchiveSubscriptionPlan;

public sealed record ArchiveSubscriptionPlanCommand : IRequest<ArchiveSubscriptionPlanResult>
{
    public required Guid PlanId { get; init; }

    [JsonIgnore]
    public string? AdminUserId { get; init; }
}
