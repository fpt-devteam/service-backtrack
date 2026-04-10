using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

public sealed class PostMatch : Entity<Guid>
{
    public required Guid LostPostId { get; set; }
    public required Guid FoundPostId { get; set; }
    public required float MatchScore { get; set; }
    public required float DistanceMeters { get; set; }
    public required double TimeGapDays { get; set; }
    public required MatchingLevel MatchingLevel { get; set; }

    public bool IsAssessed { get; set; }
    public required string AssessmentSummary { get; set; }

    // Navigation properties
    public Post LostPost { get; set; } = default!;
    public Post FoundPost { get; set; } = default!;
}
