using Backtrack.Core.Application.Usecases.Posts;

namespace Backtrack.Core.Application.Interfaces.AI;

public interface IImageAnalysisService
{
    Task<PersonalBelongingDetailInput> AnalyzePersonalBelongingAsync(
        string imageBase64, string mimeType, CancellationToken cancellationToken = default);

    Task<ElectronicDetailInput> AnalyzeElectronicAsync(
        string imageBase64, string mimeType, CancellationToken cancellationToken = default);

    Task<OtherDetailInput> AnalyzeOtherAsync(
        string imageBase64, string mimeType, CancellationToken cancellationToken = default);
}
