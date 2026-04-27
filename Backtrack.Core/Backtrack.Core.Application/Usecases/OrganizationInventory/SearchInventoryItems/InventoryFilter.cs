using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;

public sealed record InventoryFilter
{
    public PostStatus? Status { get; init; }
    public PostType? PostType { get; init; }
    public ItemCategory? Category { get; init; }
    public string? StaffId { get; init; }
    public TimeFilter? Time { get; init; }
}
