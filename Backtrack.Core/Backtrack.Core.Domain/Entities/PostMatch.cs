using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Domain.Entities;

public sealed class PostMatch : Entity<Guid>
{
    public required Guid LostPostId { get; set; }
    public required Guid FoundPostId { get; set; }
    public required float MatchScore { get; set; }
    public required float DistanceMeters { get; set; }

    // Navigation properties
    public Post LostPost { get; set; } = default!;
    public Post FoundPost { get; set; } = default!;
}
