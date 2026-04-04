using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Organizations.CreateOrganization;
using Backtrack.Core.Application.Usecases.Organizations.GetMyOrganizations;
using Backtrack.Core.Application.Usecases.Organizations.GetOrganization;
using Backtrack.Core.Application.Usecases.Organizations.GetSettingOrganizationById;
using Backtrack.Core.Application.Usecases.Organizations.UpdateOrganization;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("orgs")]
[Produces("application/json")]
public class OrganizationController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrganizationAsync(
        [FromBody] CreateOrganizationCommand command, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { UserId = userId };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpGet("{orgId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationAsync(
        [FromRoute] Guid orgId, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetOrganizationQuery(orgId, userId);
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("{orgId:guid}/settings/public")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationSettingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSettingOrganizationByIdAsync(
        [FromRoute] Guid orgId, CancellationToken cancellationToken)
    {
        var query = new GetSettingOrganizationByIdQuery(orgId);
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<List<OrganizationResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrganizationsAsync(CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetMyOrganizationsQuery(userId);
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPut("{orgId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrganizationAsync(
        [FromRoute] Guid orgId,
        [FromBody] UpdateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { OrgId = orgId, UserId = userId };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }
}
