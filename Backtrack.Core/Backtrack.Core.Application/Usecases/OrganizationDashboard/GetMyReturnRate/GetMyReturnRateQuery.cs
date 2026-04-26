using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetMyReturnRate;

public sealed record GetMyReturnRateQuery : IRequest<MyReturnRateResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid OrgId { get; init; }
}
