using Backtrack.Core.Application.ImageAnalysis.Commands.AnalyzeImage;
using Backtrack.Core.Application.ImageAnalysis.Common;
using Backtrack.Core.WebApi.Contracts.ImageAnalysis.Requests;
using Backtrack.Core.WebApi.Contracts.ImageAnalysis.Responses;

namespace Backtrack.Core.WebApi.Mappings;

public static class ImageAnalysisMappings
{
    // ==================== Request to Command ====================

    public static AnalyzeImageCommand ToCommand(this AnalyzeImageRequest request)
    {
        return new AnalyzeImageCommand
        {
            ImageBase64 = request.ImageBase64,
            MimeType = request.MimeType
        };
    }

    // ==================== Result to Response ====================

    public static AnalyzeImageResponse ToResponse(this ImageAnalysisResult result)
    {
        return new AnalyzeImageResponse
        {
            ItemName = result.ItemName,
            Description = result.Description
        };
    }
}
