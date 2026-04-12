using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Application.Usecases.PostExplorations;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SearchInventories;

public sealed record InventoryFilter
{
    public PostStatus? Status { get; init; }
    public ItemCategory? Category { get; init; }
    public string? StaffId { get; init; }
    public TimeFilter? Time { get; init; }
}
