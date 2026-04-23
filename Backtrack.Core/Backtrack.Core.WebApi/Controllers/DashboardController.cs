using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.Admin.GetDashboardKpi;
using Backtrack.Core.Application.Usecases.Admin.GetPostMonthly;
using Backtrack.Core.Application.Usecases.Admin.GetRecentActivity;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("super-admin/dashboard")]
[Produces("application/json")]
public class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("kpi")]
    [ProducesResponseType(typeof(ApiResponse<DashboardKpiResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardKpiAsync(CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(new GetDashboardKpiQuery(adminUserId), cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("post-monthly")]
    [ProducesResponseType(typeof(ApiResponse<List<PostMonthlyResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPostMonthlyAsync(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new GetPostMonthlyQuery { AdminUserId = adminUserId, Months = months },
            cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("recent-activity")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RecentActivityItemResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentActivityAsync(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new GetRecentActivityQuery { AdminUserId = adminUserId, Status = status, Page = page, PageSize = pageSize },
            cancellationToken);
        return this.ApiOk(result);
    }
}
