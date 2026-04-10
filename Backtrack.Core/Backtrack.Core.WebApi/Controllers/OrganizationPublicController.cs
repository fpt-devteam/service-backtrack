using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Organizations.CheckSlugExist;
using Backtrack.Core.Application.Usecases.Organizations.GetAllOrganizations;
using Backtrack.Core.Application.Usecases.Organizations.GetOrganizationBySlug;
using Backtrack.Core.Application.Usecases.Organizations.GetOrganizationSetting;
using Backtrack.Core.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("orgs/public")]
[Produces("application/json")]
public class OrganizationPublicController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<OrganizationResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrganizationsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllOrganizationsQuery(page, pageSize);
        var (items, total) = await mediator.Send(query, cancellationToken);
        var response = PagedResponse<OrganizationResult>.Create(items, page, pageSize, total);
        return this.ApiOk(response);
    }

    [HttpGet("slug/{slug}/exists")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckSlugExistAsync(
        [FromRoute] string slug, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CheckSlugExistQuery(slug), cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("{orgId:guid}/settings")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationSettingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationSettingAsync(
        [FromRoute] Guid orgId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrganizationSettingQuery(orgId), cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationBySlugAsync(
        [FromRoute] string slug,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrganizationBySlugQuery(slug);
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }
}
