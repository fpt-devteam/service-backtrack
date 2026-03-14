namespace Backtrack.Core.Application.Usecases.Posts.GetPostMatchingStatus;

public sealed record GetPostMatchingStatusResult
{
    public required Guid PostId { get; init; }
    public required string EmbeddingStatus { get; init; }
    public required string MatchingStatus { get; init; }
}
