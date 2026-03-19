namespace Backtrack.Core.Application.Usecases.PostMatchings.GetPostMatchingStatus;

public sealed record GetPostMatchingStatusResult
{
    public required Guid PostId { get; init; }
    public required string EmbeddingStatus { get; init; }
    public required string MatchingStatus { get; init; }
}
