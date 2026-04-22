namespace Backtrack.Core.Domain.Entities;

public sealed class PostMatch : Entity<Guid>
{
    public required Guid LostPostId { get; set; }
    public required Guid FoundPostId { get; set; }
    public required double Score { get; set; }
    public List<MatchEvidence> Evidence { get; set; } = [];
    public MatchStatus Status { get; set; } = MatchStatus.Pending;

    public Post LostPost { get; set; } = default!;
    public Post FoundPost { get; set; } = default!;
}

public record MatchEvidence(
    string Key,              // "holder_name", "brand", "color", "location", "time_gap" …
    MatchStrength Strength,  // Strong | Partial | Weak | Mismatch
    string? DisplayValue,    // human-readable value, e.g. "NGÔ ĐỨC BÌNH" or "1.2 km apart"
    string? Note             // optional explanation
);

public enum MatchStrength { Strong, Partial, Weak, Mismatch }

public enum MatchStatus
{
    Pending,
    Confirmed,
    Rejected
}
