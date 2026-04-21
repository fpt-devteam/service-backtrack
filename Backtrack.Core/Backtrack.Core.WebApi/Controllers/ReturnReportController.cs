using Microsoft.AspNetCore.Mvc;
using MediatR;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Utils;
using Backtrack.Core.Application.Usecases.ReturnReport;
using Backtrack.Core.Application.Usecases.ReturnReport.InitiateC2CReturnReport;
using Backtrack.Core.Application.Usecases.ReturnReport.FinderDeliveredC2CReturnReport;
using Backtrack.Core.Application.Usecases.ReturnReport.CreateOrgReturnReport;
using Backtrack.Core.Application.Usecases.ReturnReport.GetC2CReturnReportById;
using Backtrack.Core.Application.Usecases.ReturnReport.OwnerConfirmC2CReturnReport;
using Backtrack.Core.Application.Usecases.ReturnReport.RejectC2CReturnReport;
using Backtrack.Core.Application.Usecases.ReturnReport.GetC2CReturnReportsByUserId;
using Backtrack.Core.Application.Usecases.ReturnReport.GetC2CReturnReportsByPartnerId;
using Backtrack.Core.Application.Usecases.ReturnReport.GetOrgReturnReports;
using Backtrack.Core.Application.Usecases.ReturnReport.GetOrgReturnReportById;
using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("return-reports")]
[Produces("application/json")]
public class ReturnReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReturnReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Create a C2C return report (user to user)</summary>
    [HttpPost("c2c")]
    [ProducesResponseType(typeof(ApiResponse<C2CReturnReportResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InitiateC2CReturnReportAsync(
        [FromBody] InitiateC2CReturnReportCommand command, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { InitiatorId = userId };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    /// <summary>Get org return reports. Admins see all; staff see only their own.</summary>
    [HttpGet("org/{orgId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrgReturnReportResult>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrgReturnReportsAsync(
        [FromRoute] Guid orgId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetOrgReturnReportsQuery { UserId = userId, OrgId = orgId, Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>Get org return report detail by ID</summary>
    [HttpGet("org/{orgId:guid}/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrgReturnReportResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrgReturnReportByIdAsync(
        [FromRoute] Guid orgId, [FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetOrgReturnReportByIdQuery { UserId = userId, OrgId = orgId, ReturnReportId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>Create an org return report (staff on behalf of organization)</summary>
    [HttpPost("org/{orgId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrgReturnReportResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrgReturnReportAsync(
        [FromRoute] Guid orgId,
        [FromBody] CreateOrgReturnReportCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { UserId = userId, OrgId = orgId };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    /// <summary>Get all C2C return reports for the current user</summary>
    [HttpGet("c2c")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<C2CReturnReportResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetC2CReturnReportsByUserIdAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] C2CReturnReportStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetC2CReturnReportsByUserIdQuery { UserId = userId, Page = page, PageSize = pageSize, Status = status };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>Get a C2C return report by ID</summary>
    [HttpGet("c2c/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<C2CReturnReportResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetC2CReturnReportByIdAsync(
        [FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetC2CReturnReportByIdQuery { UserId = userId, C2CReturnReportId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>Get all C2C return reports between the current user and a partner</summary>
    [HttpGet("c2c/partner/{partnerId}")]
    [ProducesResponseType(typeof(ApiResponse<List<C2CReturnReportResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetC2CReturnReportsByPartnerIdAsync(
        [FromRoute] string partnerId, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetC2CReturnReportsByPartnerIdQuery { UserId = userId, PartnerId = partnerId };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>Finder marks item as delivered. Owner must then confirm or reject.</summary>
    [HttpPatch("c2c/{id:guid}/deliver")]
    [ProducesResponseType(typeof(ApiResponse<C2CReturnReportResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinderDeliveredC2CReturnReportAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var command = new FinderDeliveredC2CReturnReportCommand { UserId = userId, C2CReturnReportId = id };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>Reject a C2C return report. Can be called by either the finder or the owner at any non-terminal status.</summary>
    [HttpPatch("c2c/{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<C2CReturnReportResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectC2CReturnReportAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var command = new RejectC2CReturnReportCommand { UserId = userId, C2CReturnReportId = id };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>Confirm a C2C return report. Must be called by the counterpart of whoever activated it.</summary>
    [HttpPatch("c2c/{id:guid}/confirm")]
    [ProducesResponseType(typeof(ApiResponse<C2CReturnReportResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmC2CReturnReportAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var command = new OwnerConfirmC2CReturnReportCommand { UserId = userId, C2CReturnReportId = id };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }
}
