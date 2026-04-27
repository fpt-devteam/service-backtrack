using Backtrack.Core.Application.Usecases.Admin.GetOrganizations;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("super-admin/organizations")]
[Produces("application/json")]
public class OrganizationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<OrganizationsResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationsAsync(
        [FromQuery] string? search = null,
        [FromQuery] OrganizationStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortOrder = "desc",
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new GetOrganizationsQuery
            {
                AdminUserId = adminUserId,
                Search      = search,
                Status      = status,
                Page        = page,
                PageSize    = pageSize,
                SortBy      = sortBy,
                SortOrder   = sortOrder,
            },
            cancellationToken);
        return this.ApiOk(result);
    }
}
