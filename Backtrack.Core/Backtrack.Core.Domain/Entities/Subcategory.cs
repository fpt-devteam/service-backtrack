using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public sealed class Subcategory : Entity<Guid>
{
    public required ItemCategory Category { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
