using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Domain.Entities;

public sealed class PostMatch : Entity<Guid>
{
    public required Guid LostPostId { get; set; }
    public required Guid FoundPostId { get; set; }
    public required float MatchScore { get; set; }
    public required float DistanceMeters { get; set; }

    // Computed deterministically from MatchScore via PostMatchingCriteria.ComputeMatchingLevel
    public required MatchingLevel MatchingLevel { get; set; }

    // Per-criteria scores (0-100), computed deterministically from embeddings + formulas
    public required int DescriptionScore { get; set; }
    public required int VisualScore { get; set; }
    public required int LocationScore { get; set; }
    public required int TimeWindowScore { get; set; }

    // Whether LLM reasoning has been run for this match
    public bool IsAssessed { get; set; }

    // LLM reasoning
    public string? AssessmentSummary { get; set; }
    public PostMatchCriteriaAssessment? CriteriaAssessment { get; set; }

    // Navigation properties
    public Post LostPost { get; set; } = default!;
    public Post FoundPost { get; set; } = default!;
}
