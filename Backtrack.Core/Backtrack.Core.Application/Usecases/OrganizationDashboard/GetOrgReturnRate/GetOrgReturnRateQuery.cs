using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetOrgReturnRate;

public sealed record GetOrgReturnRateQuery : IRequest<OrgReturnRateResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid OrgId { get; init; }
}
