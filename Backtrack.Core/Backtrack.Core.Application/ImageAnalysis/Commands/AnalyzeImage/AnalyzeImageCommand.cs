using Backtrack.Core.Application.ImageAnalysis.Common;
using MediatR;

namespace Backtrack.Core.Application.ImageAnalysis.Commands.AnalyzeImage;

/// <summary>
/// Command to analyze an image and extract item information for post creation.
/// </summary>
public sealed record AnalyzeImageCommand : IRequest<ImageAnalysisResult>
{
    /// <summary>
    /// Base64 encoded image data.
    /// </summary>
    public required string ImageBase64 { get; init; }

    /// <summary>
    /// MIME type of the image (e.g., image/jpeg, image/png, image/webp).
    /// </summary>
    public required string MimeType { get; init; }
}
