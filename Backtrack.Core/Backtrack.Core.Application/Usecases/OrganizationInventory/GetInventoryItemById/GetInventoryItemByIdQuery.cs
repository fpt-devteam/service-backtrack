using Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;
using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.GetInventoryItemById;

public sealed record GetInventoryItemByIdQuery : IRequest<InventoryItemResult>
{
    public required Guid PostId { get; init; }
    [JsonIgnore] public string? UserId { get; init; }
    [JsonIgnore] public Guid OrgId { get; init; }
}
