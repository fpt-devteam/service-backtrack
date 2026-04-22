using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.Admin;
using Backtrack.Core.Application.Usecases.Admin.GetDashboardOverview;
using Backtrack.Core.Application.Usecases.Admin.GetGrowthChart;
using Backtrack.Core.Application.Usecases.Admin.GetOrganizationDetail;
using Backtrack.Core.Application.Usecases.Admin.GetOrganizations;
using Backtrack.Core.Application.Usecases.Admin.GetPostOverview;
using Backtrack.Core.Application.Usecases.Admin.GetRevenueOverview;
using Backtrack.Core.Application.Usecases.Admin.GetUserDetail;
using Backtrack.Core.Application.Usecases.Admin.GetUsers;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("admin")]
[Produces("application/json")]
public class AdminController(IMediator mediator) : ControllerBase
{
    // ── Dashboard ────────────────────────────────────────────────────────────

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardOverviewResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(new GetDashboardOverviewQuery(adminUserId), cancellationToken);
        return this.ApiOk(result);
    }

    // ── Revenue ───────────────────────────────────────────────────────────────

    [HttpGet("revenue")]
    [ProducesResponseType(typeof(ApiResponse<RevenueOverviewResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueOverviewAsync(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(new GetRevenueOverviewQuery { AdminUserId = adminUserId, Months = months }, cancellationToken);
        return this.ApiOk(result);
    }

    // ── Posts ─────────────────────────────────────────────────────────────────

    [HttpGet("posts/overview")]
    [ProducesResponseType(typeof(ApiResponse<PostDetailOverviewResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPostOverviewAsync(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(new GetPostOverviewQuery { AdminUserId = adminUserId, Months = months }, cancellationToken);
        return this.ApiOk(result);
    }

    // ── Growth charts ─────────────────────────────────────────────────────────

    [HttpGet("users/chart")]
    [ProducesResponseType(typeof(ApiResponse<GrowthChartResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserGrowthChartAsync(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new GetGrowthChartQuery { AdminUserId = adminUserId, Entity = GrowthChartEntity.Users, Months = months },
            cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("orgs/chart")]
    [ProducesResponseType(typeof(ApiResponse<GrowthChartResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrgGrowthChartAsync(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new GetGrowthChartQuery { AdminUserId = adminUserId, Entity = GrowthChartEntity.Orgs, Months = months },
            cancellationToken);
        return this.ApiOk(result);
    }

    // ── Users ────────────────────────────────────────────────────────────────

    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AdminUserSummaryResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsersAsync(
        [FromQuery] string? search,
        [FromQuery] UserStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetUsersQuery
        {
            AdminUserId = adminUserId,
            Search      = search,
            Status      = status,
            Page        = page,
            PageSize    = pageSize
        };
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDetailResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserDetailAsync(
        [FromRoute] string userId,
        [FromQuery] int billingPage = 1,
        [FromQuery] int billingPageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetUserDetailQuery
        {
            AdminUserId     = adminUserId,
            TargetUserId    = userId,
            BillingPage     = billingPage,
            BillingPageSize = billingPageSize
        };
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    // ── Organizations ────────────────────────────────────────────────────────

    [HttpGet("orgs")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationsResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationsAsync(
        [FromQuery] string? search,
        [FromQuery] OrganizationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetOrganizationsQuery
        {
            AdminUserId = adminUserId,
            Search      = search,
            Status      = status,
            Page        = page,
            PageSize    = pageSize
        };
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("orgs/{orgId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AdminOrgDetailResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationDetailAsync(
        [FromRoute] Guid orgId,
        [FromQuery] int billingPage = 1,
        [FromQuery] int billingPageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetOrganizationDetailQuery
        {
            AdminUserId     = adminUserId,
            OrgId           = orgId,
            BillingPage     = billingPage,
            BillingPageSize = billingPageSize
        };
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }
}
