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
    Task<ImageAnalysisOutput> AnalyzeImageAsync(
        string imageBase64,
        string mimeType,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Output from image analysis containing structured item information.
/// </summary>
public sealed record ImageAnalysisOutput
{
    /// <summary>
    /// The identified name of the item in the image.
    /// </summary>
    public required string ItemName { get; init; }

    /// <summary>
    /// Detailed description including category, color, brand, condition, and distinctive marks.
    /// </summary>
    public required string Description { get; init; }
}
