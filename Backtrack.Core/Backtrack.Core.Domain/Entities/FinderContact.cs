namespace Backtrack.Core.Domain.Entities;

public sealed class FinderContact : Entity<Guid>
{
    public required Guid InventoryId { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? NationalId { get; set; }
    public string? OrgMemberId { get; set; }

    public OrganizationInventory Inventory { get; set; } = default!;
}
