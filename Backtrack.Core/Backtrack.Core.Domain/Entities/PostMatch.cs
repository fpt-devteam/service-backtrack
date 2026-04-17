namespace Backtrack.Core.Domain.Entities;

public sealed class PostMatch : Entity<Guid>
{
    public required Guid SourcePostId { get; set; }
    public required Guid CandidatePostId { get; set; }
    public required double Score { get; set; }          // 0.0 - 1.0
    public required string MatchReason { get; set; }    // "rrf_embedding", "card_hash_exact"...
    public MatchStatus Status { get; set; } = MatchStatus.Pending;

    public Post SourcePost { get; set; } = default!;
    public Post CandidatePost { get; set; } = default!;
}

public enum MatchStatus
{
    Pending,
    Confirmed,
    Rejected
}
