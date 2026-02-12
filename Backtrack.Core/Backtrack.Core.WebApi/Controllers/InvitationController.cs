using Backtrack.Core.Application.Usecases.Organizations.Commands.CreateInvitation;
using Backtrack.Core.Application.Usecases.Organizations.Commands.JoinByInvitation;
using Backtrack.Core.Application.Usecases.Organizations.Queries.CheckInvitation;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("invitations")]
[Produces("application/json")]
public class InvitationController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvitationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateInvitationResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInvitationAsync(
        [FromBody] CreateInvitationCommand command, CancellationToken cancellationToken)
    {
        string userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        string userName = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthName);
        command = command with { UserId = userId, UserName = userName };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpPost("check")]
    [ProducesResponseType(typeof(ApiResponse<CheckInvitationResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckInvitationAsync(
        [FromBody] CheckInvitationQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPost("join")]
    [ProducesResponseType(typeof(ApiResponse<JoinByInvitationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> JoinByInvitationAsync(
        [FromBody] JoinByInvitationCommand command, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var userEmail = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthEmail);
        command = command with { UserId = userId, UserEmail = userEmail };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }
}
