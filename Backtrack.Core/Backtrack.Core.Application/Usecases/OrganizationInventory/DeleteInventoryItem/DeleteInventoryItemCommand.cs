using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.DeleteInventoryItem;

public sealed record DeleteInventoryItemCommand : IRequest
{
    [JsonIgnore] public Guid PostId { get; init; }
    [JsonIgnore] public string UserId { get; init; } = string.Empty;
    [JsonIgnore] public Guid OrgId { get; init; }
}
