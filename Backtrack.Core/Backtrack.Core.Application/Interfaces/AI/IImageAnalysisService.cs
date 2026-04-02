using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Interfaces.AI;

/// <summary>
/// Interface for analyzing images using AI to extract item information.
/// Used to help users auto-fill post creation forms.
/// </summary>
public interface IImageAnalysisService
{
    /// <summary>
    /// Analyzes an image and extracts structured item information.
    /// </summary>
    /// <param name="imageBase64">Base64 encoded image data</param>
    /// <param name="mimeType">MIME type of the image (e.g., image/jpeg, image/png)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis result containing item name and description</returns>
    Task<PostItem> AnalyzeImageAsync(
        string imageBase64,
        string mimeType,
        CancellationToken cancellationToken = default);
}
