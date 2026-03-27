using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Domain.Entities;

public sealed class OrgFormTemplate : Entity<Guid>
{
    public required Guid OrgId { get; set; }
    public required List<FormFieldDefinition> Fields { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = default!;
}
