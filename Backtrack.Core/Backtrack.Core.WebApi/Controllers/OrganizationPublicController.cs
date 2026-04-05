using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Organizations.GetAllOrganizations;
using Backtrack.Core.Application.Usecases.Organizations.GetOrganizationPublic;
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

    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationBySlugAsync(
        [FromRoute] string slug,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrganizationPublicQuery(slug);
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }
}
