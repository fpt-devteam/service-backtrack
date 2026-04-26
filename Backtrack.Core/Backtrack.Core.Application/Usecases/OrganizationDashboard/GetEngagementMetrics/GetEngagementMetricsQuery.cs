using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetEngagementMetrics;

public sealed record GetEngagementMetricsQuery : IRequest<EngagementMetricsResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid OrgId { get; init; }
}
