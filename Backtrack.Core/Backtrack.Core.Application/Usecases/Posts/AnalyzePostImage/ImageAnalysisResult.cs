namespace Backtrack.Core.Application.Usecases.Posts.AnalyzePostImage;

/// <summary>
/// Unified result from AnalyzePostImageCommand. Only one of the nullable detail fields
/// is populated, matching the Category used for analysis.
/// </summary>
public sealed record ImageAnalysisResult
{
    public required string Category { get; init; }
    public PersonalBelongingDetailInput? PersonalBelonging { get; init; }
    public ElectronicDetailInput? Electronic { get; init; }
    public OtherDetailInput? Other { get; init; }
    public CardDetailInput? Card { get; init; }
    public IReadOnlyList<string>? Warnings { get; init; }
}
