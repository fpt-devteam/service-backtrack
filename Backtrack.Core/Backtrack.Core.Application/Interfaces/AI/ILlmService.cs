using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Interfaces.AI;

public sealed record PostMatchContext
{
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public string? DisplayAddress { get; init; }
    public string? ImageBase64 { get; init; }
    public string? ImageMimeType { get; init; }
}

/// <summary>Pre-computed 0-100 scores for each criterion, passed to LLM for reasoning.</summary>
public sealed record PostMatchScores
{
    public required int DescriptionScore { get; init; }
    public required int VisualScore { get; init; }
    public required int LocationScore { get; init; }
    public required int TimeWindowScore { get; init; }
}

public sealed record PostMatchAssessment
{
    public required PostMatchCriteriaAssessment Criteria { get; init; }
    public required string Summary { get; init; }
}

public interface ILlmService
{
    Task<PostMatchAssessment> AssessPostMatchAsync(
        PostMatchContext lostPost,
        PostMatchContext foundPost,
        PostMatchScores scores,
        float distanceMeters,
        CancellationToken cancellationToken = default);
}
