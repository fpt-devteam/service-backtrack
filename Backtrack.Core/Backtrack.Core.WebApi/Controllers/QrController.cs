using Backtrack.Core.Application.Usecases.QrCodes;
using Backtrack.Core.Application.Usecases.QrCodes.GetMyQrCode;
using Backtrack.Core.Application.Usecases.QrCodes.GetQrByPublicCode;
using Backtrack.Core.Application.Usecases.QrCodes.UpdateQrNote;
using Backtrack.Core.Application.Usecases.QrDesigns;
using Backtrack.Core.Application.Usecases.QrDesigns.GetMyQrDesign;
using Backtrack.Core.Application.Usecases.QrDesigns.UpdateMyQrDesign;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("qr")]
[Produces("application/json")]
public class QrController(IMediator mediator) : ControllerBase
{
    // --- QR Code ---

    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<QrCodeResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyQrCodeAsync(CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(new GetMyQrCodeQuery { UserId = userId }, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("public/{publicCode}")]
    [ProducesResponseType(typeof(ApiResponse<QrPublicResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQrByPublicCodeAsync(
        [FromRoute] string publicCode, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetQrByPublicCodeQuery { PublicCode = publicCode }, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPatch("me/note")]
    [ProducesResponseType(typeof(ApiResponse<QrCodeResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQrNoteAsync(
        [FromBody] UpdateQrNoteCommand command, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { UserId = userId };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    // --- QR Design ---

    [HttpGet("me/design")]
    [ProducesResponseType(typeof(ApiResponse<QrDesignResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyQrDesignAsync(CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(new GetMyQrDesignQuery { UserId = userId }, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPut("me/design")]
    [ProducesResponseType(typeof(ApiResponse<QrDesignResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMyQrDesignAsync(
        [FromBody] UpdateMyQrDesignCommand command, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { UserId = userId };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }
}
