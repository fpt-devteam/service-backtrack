using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Interfaces.AI;

public sealed record PostMatchContext
{
    public required string LostDescription { get; init; }
    public required string FoundDescription { get; init; }
    public required float DistanceMeters { get; init; }
    public required double TimeGapDays { get; init; }
    public required float MatchScore { get; init; }
    public required MatchingLevel MatchingLevel { get; init; }
}

public sealed record PostMatchAssessment
{
    public required string Summary { get; init; }
}

public interface IPostMatchAssessor
{
    Task<PostMatchAssessment> AssessAsync(
        PostMatchContext context,
        CancellationToken cancellationToken = default);
}
