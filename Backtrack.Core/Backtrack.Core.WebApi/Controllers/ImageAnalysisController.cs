using Backtrack.Core.Application.Usecases.ImageAnalysis.Commands.AnalyzeImage;
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
    /// <param name="command">The image data and MIME type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted item information including name and description</returns>
    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeImageAsync(
        [FromBody] AnalyzeImageCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }
}
