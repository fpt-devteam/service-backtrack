using Microsoft.AspNetCore.Mvc;
using MediatR;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Utils;
using Backtrack.Core.Application.Usecases.Handovers;
using Backtrack.Core.Application.Usecases.Handovers.CreateP2PHandover;
using Backtrack.Core.Application.Usecases.Handovers.CreateOrgHandover;
using Backtrack.Core.Application.Usecases.Handovers.GetHandoverById;
using Backtrack.Core.Application.Usecases.Handovers.GetHandoverByToken;
using Backtrack.Core.Application.Usecases.Handovers.OwnerConfirmHandover;
using Backtrack.Core.Application.Usecases.Handovers.StaffConfirmHandover;

namespace Backtrack.Core.WebApi.Controllers;

/// <summary>
/// Manages handover operations between finders and owners
/// </summary>
[ApiController]
[Route("handovers")]
[Produces("application/json")]
public class HandoverController : ControllerBase
{
    private readonly IMediator _mediator;

    public HandoverController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a P2P handover (user to user)
    /// </summary>
    /// <param name="command">P2P handover creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created handover</returns>
    /// <response code="201">Handover created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Post not found</response>
    /// <response code="409">Active handover already exists</response>
    [HttpPost("p2p")]
    [ProducesResponseType(typeof(ApiResponse<HandoverResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateP2PHandoverAsync(
        [FromBody] CreateP2PHandoverCommand command, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { FinderId = userId };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    /// <summary>
    /// Create an org handover (staff to owner)
    /// </summary>
    /// <param name="orgId">Organization ID</param>
    /// <param name="command">Org handover creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created handover</returns>
    /// <response code="201">Handover created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="403">Not a member of the organization</response>
    /// <response code="404">Organization or post not found</response>
    /// <response code="409">Active handover already exists</response>
    [HttpPost("org/{orgId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HandoverResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrgHandoverAsync(
        [FromRoute] Guid orgId,
        [FromBody] CreateOrgHandoverCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { UserId = userId, OrgId = orgId };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    /// <summary>
    /// Get handover by ID (authenticated)
    /// </summary>
    /// <param name="id">Handover ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The handover details</returns>
    /// <response code="200">Handover found</response>
    /// <response code="403">Not authorized to view this handover</response>
    /// <response code="404">Handover not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HandoverResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHandoverByIdAsync(
        [FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetHandoverByIdQuery { UserId = userId, HandoverId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Get handover by token (public, no auth required)
    /// </summary>
    /// <param name="token">Handover token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The handover details with form template</returns>
    /// <response code="200">Handover found</response>
    /// <response code="400">Invalid or expired token</response>
    /// <response code="404">Handover not found</response>
    [HttpGet("token/{token}")]
    [ProducesResponseType(typeof(ApiResponse<HandoverDetailResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHandoverByTokenAsync(
        [FromRoute] string token, CancellationToken cancellationToken)
    {
        var query = new GetHandoverByTokenQuery(token);
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Owner confirms handover
    /// </summary>
    /// <remarks>
    /// For P2P handovers: confirms and closes the handover immediately.
    /// For Org handovers: submits owner form data, pending staff confirmation.
    /// </remarks>
    /// <param name="id">Handover ID</param>
    /// <param name="command">Owner confirmation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated handover</returns>
    /// <response code="200">Handover confirmed</response>
    /// <response code="400">Invalid handover state or missing form data</response>
    /// <response code="403">Not the owner of this handover</response>
    /// <response code="404">Handover not found</response>
    [HttpPatch("{id:guid}/owner-confirm")]
    [ProducesResponseType(typeof(ApiResponse<HandoverResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OwnerConfirmHandoverAsync(
        [FromRoute] Guid id,
        [FromBody] OwnerConfirmHandoverCommand command,
        CancellationToken cancellationToken)
    {
        // User ID is optional for org handovers (owner may not have account)
        string? userId = null;
        try
        {
            userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        }
        catch
        {
            // Ignore - user may not be authenticated for org handovers
        }

        command = command with { UserId = userId, HandoverId = id };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Staff confirms handover (org handovers only)
    /// </summary>
    /// <remarks>
    /// Only valid for Org type handovers after owner has confirmed.
    /// </remarks>
    /// <param name="id">Handover ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated handover</returns>
    /// <response code="200">Handover confirmed</response>
    /// <response code="400">Invalid handover state or type</response>
    /// <response code="403">Not authorized to confirm this handover</response>
    /// <response code="404">Handover not found</response>
    [HttpPatch("{id:guid}/staff-confirm")]
    [ProducesResponseType(typeof(ApiResponse<HandoverResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StaffConfirmHandoverAsync(
        [FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var command = new StaffConfirmHandoverCommand { UserId = userId, HandoverId = id };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }
}
