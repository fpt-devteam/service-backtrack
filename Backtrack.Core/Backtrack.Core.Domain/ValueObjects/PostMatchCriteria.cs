namespace Backtrack.Core.Domain.ValueObjects;

public sealed record CriterionPoint
{
    public required string Label { get; init; }
    public required string Detail { get; init; }
}

public sealed record PostMatchCriteriaAssessment
{
    public required CriterionPoint[] VisualAnalysis { get; init; }
    public required CriterionPoint[] Description { get; init; }
    public required CriterionPoint[] Location { get; init; }
    public required CriterionPoint[] TimeWindow { get; init; }
}
