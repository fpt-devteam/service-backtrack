using Backtrack.Core.Application.Usecases.OrganizationDashboard.GetEngagementMetrics;
using Backtrack.Core.Application.Usecases.OrganizationDashboard.GetMyReturnRate;
using Backtrack.Core.Application.Usecases.OrganizationDashboard.GetOrgPostStats;
using Backtrack.Core.Application.Usecases.OrganizationDashboard.GetStaffDashboardStats;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("orgs/{orgId:guid}/staff/dashboard")]
[Produces("application/json")]
public class OrganizationStaffDashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<StaffDashboardStatsResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStaffDashboardStatsAsync(
        [FromRoute] Guid orgId,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new GetStaffDashboardStatsQuery { OrgId = orgId, UserId = userId },
            cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("post-stats")]
    [ProducesResponseType(typeof(ApiResponse<OrgPostStatsResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrgPostStatsAsync(
        [FromRoute] Guid orgId,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new GetOrgPostStatsQuery { OrgId = orgId, UserId = userId },
            cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("engagement")]
    [ProducesResponseType(typeof(ApiResponse<EngagementMetricsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEngagementMetricsAsync(
        [FromRoute] Guid orgId,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new GetEngagementMetricsQuery { OrgId = orgId, UserId = userId },
            cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("return-rate")]
    [ProducesResponseType(typeof(ApiResponse<MyReturnRateResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyReturnRateAsync(
        [FromRoute] Guid orgId,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new GetMyReturnRateQuery { OrgId = orgId, UserId = userId },
            cancellationToken);
        return this.ApiOk(result);
    }
}
