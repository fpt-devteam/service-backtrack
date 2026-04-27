using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Admin.UpdateSubscriptionPlanFeatures;

public sealed record UpdateSubscriptionPlanFeaturesCommand : IRequest<UpdateSubscriptionPlanFeaturesResult>
{
    [JsonIgnore]
    public Guid PlanId { get; init; }
    public required string[] Features { get; init; }

    [JsonIgnore]
    public string? AdminUserId { get; init; }
}
