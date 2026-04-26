namespace Backtrack.Core.Domain.Entities;

public sealed class PostMatch : Entity<Guid>
{
    public required Guid LostPostId { get; set; }
    public required Guid FoundPostId { get; set; }
    public required double Score { get; set; }
    public List<MatchEvidence> Evidence { get; set; } = [];
    public required string Reasoning { get; set; }
    public required MatchStatus Status { get; set; }
    public required double DistanceMeters { get; set; }
    public required double TimeGapDays { get; set; }

    public Post LostPost { get; set; } = default!;
    public Post FoundPost { get; set; } = default!;
}

public record MatchEvidence(
    string Key,
    MatchStrength Strength,
    string LostValue,
    string FoundValue,
    string? Note);


public enum MatchStrength { Strong, Partial, Mismatch }

public enum MatchStatus
{
    ReadyToShow,
    RejectedByAI,
}
