using Backtrack.Core.WebApi.Contracts.ImageAnalysis.Requests;
using Backtrack.Core.WebApi.Mappings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

/// <summary>
/// Controller for AI-powered image analysis operations.
/// </summary>
[ApiController]
[Route("image-analysis")]
public class ImageAnalysisController : ControllerBase
{
    private readonly IMediator _mediator;

    public ImageAnalysisController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Analyzes an image using AI to extract item information for post creation.
    /// Returns structured data including item name and detailed description.
    /// </summary>
    /// <param name="request">The image data and MIME type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted item information including name and description</returns>
    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeImageAsync(
        [FromBody] AnalyzeImageRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await _mediator.Send(command, cancellationToken);
        var response = result.ToResponse();

        return this.ApiOk(response);
    }
}
