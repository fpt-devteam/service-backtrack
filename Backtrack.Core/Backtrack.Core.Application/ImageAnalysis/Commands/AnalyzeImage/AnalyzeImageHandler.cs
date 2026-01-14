using Backtrack.Core.Application.Common.Interfaces.AI;
using Backtrack.Core.Application.ImageAnalysis.Common;
using MediatR;

namespace Backtrack.Core.Application.ImageAnalysis.Commands.AnalyzeImage;

/// <summary>
/// Handler for analyzing images and extracting item information.
/// </summary>
public sealed class AnalyzeImageHandler : IRequestHandler<AnalyzeImageCommand, ImageAnalysisResult>
{
    private readonly IImageAnalysisService _imageAnalysisService;

    public AnalyzeImageHandler(IImageAnalysisService imageAnalysisService)
    {
        _imageAnalysisService = imageAnalysisService;
    }

    public async Task<ImageAnalysisResult> Handle(AnalyzeImageCommand command, CancellationToken cancellationToken)
    {
        var analysisOutput = await _imageAnalysisService.AnalyzeImageAsync(
            command.ImageBase64,
            command.MimeType,
            cancellationToken);

        return new ImageAnalysisResult
        {
            ItemName = analysisOutput.ItemName,
            Description = analysisOutput.Description
        };
    }
}
