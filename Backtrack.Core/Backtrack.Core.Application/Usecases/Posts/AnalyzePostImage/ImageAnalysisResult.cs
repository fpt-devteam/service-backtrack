namespace Backtrack.Core.Application.Usecases.Posts.AnalyzePostImage;

/// <summary>
/// Unified result from AnalyzePostImageCommand. Only one of the nullable detail fields
/// is populated, matching the Category used for analysis.
/// </summary>
public sealed record ImageAnalysisResult
{
    public required string Category { get; init; }
    public PersonalBelongingDetailDto? PersonalBelonging { get; init; }
    public ElectronicDetailDto? Electronic { get; init; }
    public OtherDetailDto? Other { get; init; }
    public CardDetailDto? Card { get; init; }
    public IReadOnlyList<string>? Warnings { get; init; }
}
