using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.Admin;
using Backtrack.Core.Application.Usecases.Admin.ArchiveSubscriptionPlan;
using Backtrack.Core.Application.Usecases.Admin.GetSubscriptionPlans;
using Backtrack.Core.Application.Usecases.Admin.CreateSubscriptionPlan;
using Backtrack.Core.Application.Usecases.Admin.GetDashboardOverview;
using Backtrack.Core.Application.Usecases.Admin.GetGrowthChart;
using Backtrack.Core.Application.Usecases.Admin.GetOrganizationDetail;
using Backtrack.Core.Application.Usecases.Admin.GetOrganizations;
using Backtrack.Core.Application.Usecases.Admin.GetPostOverview;
using Backtrack.Core.Application.Usecases.Admin.GetRevenueOverview;
using Backtrack.Core.Application.Usecases.Admin.GetUserDetail;
using Backtrack.Core.Application.Usecases.Admin.GetOrgDashboardStats;
using Backtrack.Core.Application.Usecases.Admin.GetOrgMonthlyActivity;
using Backtrack.Core.Application.Usecases.Admin.GetOrgStaffPerformance;
using Backtrack.Core.Application.Usecases.Admin.GetUsers;
using Backtrack.Core.Application.Usecases.Admin.UpdateSubscriptionPlanFeatures;
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

    // ── Org admin dashboard ───────────────────────────────────────────────────

    [HttpGet("orgs/{orgId:guid}/dashboard/stats")]
    [ProducesResponseType(typeof(ApiResponse<OrgDashboardStatsResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrgDashboardStatsAsync(
        [FromRoute] Guid orgId,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new GetOrgDashboardStatsQuery { OrgId = orgId, UserId = userId },
            cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("orgs/{orgId:guid}/dashboard/staff-performance")]
    [ProducesResponseType(typeof(ApiResponse<List<StaffPerformanceItemResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrgStaffPerformanceAsync(
        [FromRoute] Guid orgId,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new GetOrgStaffPerformanceQuery { OrgId = orgId, UserId = userId },
            cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("orgs/{orgId:guid}/dashboard/monthly-activity")]
    [ProducesResponseType(typeof(ApiResponse<List<MonthlyActivityPoint>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrgMonthlyActivityAsync(
        [FromRoute] Guid orgId,
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new GetOrgMonthlyActivityQuery { OrgId = orgId, UserId = userId, Months = months },
            cancellationToken);
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

    // ── Subscription Plans ────────────────────────────────────────────────────

    [HttpGet("subscription-plans")]
    [ProducesResponseType(typeof(ApiResponse<AdminSubscriptionPlansResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSubscriptionPlansAsync(CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(new GetAdminSubscriptionPlansQuery { AdminUserId = adminUserId }, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPost("subscription-plans")]
    [ProducesResponseType(typeof(ApiResponse<CreateSubscriptionPlanResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateSubscriptionPlanAsync(
        [FromBody] CreateSubscriptionPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { AdminUserId = adminUserId };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpPatch("subscription-plans/{planId:guid}/features")]
    [ProducesResponseType(typeof(ApiResponse<UpdateSubscriptionPlanFeaturesResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSubscriptionPlanFeaturesAsync(
        [FromRoute] Guid planId,
        [FromBody] UpdateSubscriptionPlanFeaturesCommand command,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { PlanId = planId, AdminUserId = adminUserId };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpDelete("subscription-plans/{planId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ArchiveSubscriptionPlanResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ArchiveSubscriptionPlanAsync(
        [FromRoute] Guid planId,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var result = await mediator.Send(
            new ArchiveSubscriptionPlanCommand { PlanId = planId, AdminUserId = adminUserId },
            cancellationToken);
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
