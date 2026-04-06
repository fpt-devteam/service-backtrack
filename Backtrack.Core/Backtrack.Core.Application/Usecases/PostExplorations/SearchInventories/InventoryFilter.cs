using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SearchInventories;

public sealed record InventoryFilter
{
    public PostStatus? Status { get; init; }
    public ItemCategory? Category { get; init; }
}
