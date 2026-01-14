using System.ComponentModel.DataAnnotations;

namespace Backtrack.Core.WebApi.Contracts.ImageAnalysis.Requests;

/// <summary>
/// Request to analyze an image and extract item information for post creation.
/// </summary>
public sealed record AnalyzeImageRequest
{
    /// <summary>
    /// Base64 encoded image data (without data URL prefix).
    /// </summary>
    [Required]
    public required string ImageBase64 { get; init; }

    /// <summary>
    /// MIME type of the image. Supported types: image/jpeg, image/png, image/webp, image/gif.
    /// </summary>
    [Required]
    public required string MimeType { get; init; }
}
