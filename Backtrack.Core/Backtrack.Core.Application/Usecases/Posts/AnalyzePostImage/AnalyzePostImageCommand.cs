using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.AnalyzePostImage;

public sealed record AnalyzePostImageCommand : IRequest<ImageAnalysisResult>
{
    public required List<string> ImageUrls { get; init; }
    public required string Category { get; init; }
}
