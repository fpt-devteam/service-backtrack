using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Organizations.GetInventoryItemById;

public sealed record GetInventoryItemByIdQuery : IRequest<OrganizationInventoryResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    public required Guid Id { get; init; }
    public required Guid OrgId { get; init; }
}
