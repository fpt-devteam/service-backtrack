using System.Text.Json.Serialization;
using Backtrack.Core.Application.Usecases;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;

public sealed record SearchInventoryItemsCommand : IRequest<PagedResult<InventoryItemResult>>
{
    [JsonIgnore] public string UserId { get; init; } = string.Empty;
    [JsonIgnore] public Guid OrgId { get; init; }
    public string? Query { get; init; }
    public InventoryFilter? Filters { get; init; }
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 10;
}
