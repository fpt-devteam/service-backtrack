using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Admin.GetOrgDashboardStats;

public sealed record GetOrgDashboardStatsQuery : IRequest<OrgDashboardStatsResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid OrgId { get; init; }
}
