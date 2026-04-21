using System.Text.Json.Serialization;
using Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;
using Backtrack.Core.Application.Usecases.Posts;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.PublishInventoryItem;

public sealed record PublishInventoryItemCommand : IRequest<InventoryItemResult>
{
    [JsonIgnore] public Guid PostId { get; init; }
    [JsonIgnore] public string UserId { get; init; } = string.Empty;
    [JsonIgnore] public Guid OrgId { get; init; }
}
