using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Usecases.Posts;

namespace Backtrack.Core.Application.Interfaces.AI;

public sealed record ItemConsistencyResult(bool MatchesSubcategory, string? Reason, string? SuggestedSubcategoryCode);

public interface IImageAnalysisService
{
    Task<PersonalBelongingDetailInput> AnalyzePersonalBelongingAsync(
        string imageBase64, string mimeType, CancellationToken cancellationToken = default);

    Task<ElectronicDetailInput> AnalyzeElectronicAsync(
        string imageBase64, string mimeType, CancellationToken cancellationToken = default);

    Task<OtherDetailInput> AnalyzeOtherAsync(
        string imageBase64, string mimeType, CancellationToken cancellationToken = default);

    Task<ItemConsistencyResult> VerifyItemConsistencyAsync(
        IReadOnlyList<FetchedImage> images,
        string subcategoryName,
        CancellationToken cancellationToken = default);
}
