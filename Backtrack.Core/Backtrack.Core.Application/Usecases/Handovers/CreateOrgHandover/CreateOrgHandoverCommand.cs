using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Handovers.CreateOrgHandover;

public sealed record CreateOrgHandoverCommand : IRequest<HandoverResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid OrgId { get; init; }

    public required Guid FinderPostId { get; init; }
}
