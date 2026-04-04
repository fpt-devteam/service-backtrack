using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Handovers.CreateOrgHandover;

public sealed record CreateOrgHandoverCommand : IRequest<HandoverResult>
{
    [JsonIgnore]
    public required string UserId { get; init; }

    [JsonIgnore]
    public required Guid OrgId { get; init; }

    public required Guid PostId { get; init; }
    public required string NameOwner { get; init; }
    public required string PhoneOwner { get; init; }
}
