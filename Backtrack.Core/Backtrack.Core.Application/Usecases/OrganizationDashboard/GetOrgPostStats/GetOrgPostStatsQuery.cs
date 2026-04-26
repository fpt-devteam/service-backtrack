using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetOrgPostStats;

public sealed record GetOrgPostStatsQuery : IRequest<OrgPostStatsResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid OrgId { get; init; }
}
