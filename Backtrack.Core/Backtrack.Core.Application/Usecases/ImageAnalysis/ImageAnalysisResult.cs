namespace Backtrack.Core.Application.Usecases.ImageAnalysis;

/// <summary>
/// Result DTO for image analysis operations.
/// Contains structured information extracted from an image to help users fill post creation forms.
/// </summary>
public sealed record ImageAnalysisResult
{
    /// <summary>
    /// The identified name of the item in the image.
    /// </summary>
    public required string ItemName { get; init; }

    /// <summary>
    /// Detailed description including category, color, brand, condition, and distinctive marks.
    /// All information is combined into a single descriptive text.
    /// </summary>
    public required string Description { get; init; }
}
