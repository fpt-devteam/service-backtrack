namespace Backtrack.Core.WebApi.Contracts.ImageAnalysis.Responses;

/// <summary>
/// Response containing extracted item information from image analysis.
/// </summary>
public sealed record AnalyzeImageResponse
{
    /// <summary>
    /// The identified name of the item (e.g., "Black Leather Wallet", "Silver iPhone 15 Pro").
    /// </summary>
    public required string ItemName { get; init; }

    /// <summary>
    /// Detailed description including category, color, brand, condition, and distinctive marks.
    /// </summary>
    public required string Description { get; init; }
}
