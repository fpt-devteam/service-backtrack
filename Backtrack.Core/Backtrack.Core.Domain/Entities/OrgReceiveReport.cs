using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Domain.Entities;

public sealed class OrgReceiveReport : Entity<Guid>
{
    public Guid OrgId { get; set; }
    public string StaffId { get; set; } = default!;
    public required Guid PostId { get; set; }
    public FinderInfo? FinderInfo { get; set; }

    public Organization? Organization { get; set; }
    public User? Staff { get; set; }
    public Post? Post { get; set; }
}
