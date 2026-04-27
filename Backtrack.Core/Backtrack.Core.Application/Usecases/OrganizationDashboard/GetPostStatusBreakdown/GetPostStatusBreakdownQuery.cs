using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetPostStatusBreakdown;

public sealed record GetPostStatusBreakdownQuery : IRequest<PostStatusBreakdownResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid OrgId { get; init; }
}
