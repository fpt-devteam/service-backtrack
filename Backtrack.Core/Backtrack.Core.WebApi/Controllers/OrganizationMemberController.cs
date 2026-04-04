using Backtrack.Core.Application.Usecases.Organizations.CheckInvitation;
using Backtrack.Core.Application.Usecases.Organizations.CreateInvitation;
using Backtrack.Core.Application.Usecases.Organizations.GetOrgMembers;
using Backtrack.Core.Application.Usecases.Organizations.GetPendingInvitations;
using Backtrack.Core.Application.Usecases.Organizations.JoinByInvitation;
using Backtrack.Core.Application.Usecases.Organizations.RemoveMember;
using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Organizations.UpdateMemberRole;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Produces("application/json")]
public class OrganizationMemberController(IMediator mediator) : ControllerBase
{
    // ── Members ─────────────────────────────────────────────────────────────

    [HttpGet("orgs/{orgId:guid}/members")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<MemberResult>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrgMembersAsync(
        [FromRoute] Guid orgId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetOrgMembersQuery { OrgId = orgId, UserId = userId, Page = page, PageSize = pageSize };
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(PagedResponse<MemberResult>.Create(result.Items, page, pageSize, result.Total));
    }

    [HttpDelete("orgs/{orgId:guid}/members/{membershipId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMemberAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid membershipId,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var command = new RemoveMemberCommand { OrgId = orgId, UserId = userId, TargetMembershipId = membershipId };
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPut("orgs/{orgId:guid}/members/{membershipId:guid}/role")]
    [ProducesResponseType(typeof(ApiResponse<MemberResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMemberRoleAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid membershipId,
        [FromBody] UpdateMemberRoleCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { OrgId = orgId, UserId = userId, TargetMembershipId = membershipId };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    // ── Invitations ──────────────────────────────────────────────────────────

    [HttpGet("invitations/pending")]
    [ProducesResponseType(typeof(ApiResponse<GetPendingInvitationsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPendingInvitationsAsync(
        [FromQuery] Guid organizationId,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetPendingInvitationsQuery { OrganizationId = organizationId, UserId = userId };
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPost("invitations")]
    [ProducesResponseType(typeof(ApiResponse<CreateInvitationResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInvitationAsync(
        [FromBody] CreateInvitationCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var userName = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthName);
        command = command with { UserId = userId, UserName = userName };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpPost("invitations/check")]
    [ProducesResponseType(typeof(ApiResponse<CheckInvitationResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckInvitationAsync(
        [FromBody] CheckInvitationQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPost("invitations/join")]
    [ProducesResponseType(typeof(ApiResponse<JoinByInvitationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> JoinByInvitationAsync(
        [FromBody] JoinByInvitationCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var userEmail = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthEmail);
        command = command with { UserId = userId, UserEmail = userEmail };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }
}
