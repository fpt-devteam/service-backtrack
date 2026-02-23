using Microsoft.AspNetCore.Mvc;
using MediatR;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Utils;
using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Organizations.Commands.CreateOrganization;
using Backtrack.Core.Application.Usecases.Organizations.Commands.UpdateOrganization;
using Backtrack.Core.Application.Usecases.Organizations.Commands.RemoveMember;
using Backtrack.Core.Application.Usecases.Organizations.Commands.UpdateMemberRole;
using Backtrack.Core.Application.Usecases.Organizations.Queries.GetOrganization;
using Backtrack.Core.Application.Usecases.Organizations.Queries.GetMyOrganizations;
using Backtrack.Core.Application.Usecases.Organizations.Queries.GetOrgMembers;

namespace Backtrack.Core.WebApi.Controllers;

/// <summary>
/// Manages organizations and their members
/// </summary>
[ApiController]
[Route("orgs")]
[Produces("application/json")]
public class OrganizationController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrganizationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new organization
    /// </summary>
    /// <remarks>
    /// The authenticated user will automatically become the organization admin.
    /// </remarks>
    /// <param name="command">Organization creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created organization</returns>
    /// <response code="201">Organization created successfully</response>
    /// <response code="400">Validation error (e.g. duplicate slug)</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrganizationAsync(
        [FromBody] CreateOrganizationCommand command, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { UserId = userId };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    /// <summary>
    /// Get an organization by ID
    /// </summary>
    /// <param name="orgId">The organization ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The organization details</returns>
    /// <response code="200">Organization found</response>
    /// <response code="404">Organization not found</response>
    [HttpGet("{orgId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationAsync(
        [FromRoute] Guid orgId, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetOrganizationQuery(orgId, userId);
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Get organizations for the authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of organizations the user belongs to</returns>
    /// <response code="200">Returns the user's organizations</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<List<OrganizationResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrganizationsAsync(CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetMyOrganizationsQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Get members of an organization
    /// </summary>
    /// <param name="orgId">The organization ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of organization members</returns>
    /// <response code="200">Returns the member list</response>
    /// <response code="403">User is not a member of this organization</response>
    /// <response code="404">Organization not found</response>
    [HttpGet("{orgId:guid}/members")]
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
        var query = new GetOrgMembersQuery
        {
            OrgId = orgId,
            UserId = userId,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);

        var response = PagedResponse<MemberResult>.Create(
            items: result.Items,
            page: page,
            pageSize: pageSize,
            totalCount: result.Total
        );
        return this.ApiOk(response);
    }

    /// <summary>
    /// Update an organization
    /// </summary>
    /// <param name="orgId">The organization ID</param>
    /// <param name="command">Updated organization details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated organization</returns>
    /// <response code="200">Organization updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="403">User is not an admin of this organization</response>
    /// <response code="404">Organization not found</response>
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
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Remove a member from an organization
    /// </summary>
    /// <remarks>
    /// Only organization admins can remove members. The last admin cannot be removed.
    /// </remarks>
    /// <param name="orgId">The organization ID</param>
    /// <param name="membershipId">The membership ID of the member to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Member removed successfully</response>
    /// <response code="400">Cannot remove the last admin</response>
    /// <response code="403">User is not an admin of this organization</response>
    /// <response code="404">Organization or membership not found</response>
    [HttpDelete("{orgId:guid}/members/{membershipId:guid}")]
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
        var command = new RemoveMemberCommand
        {
            OrgId = orgId,
            UserId = userId,
            TargetMembershipId = membershipId
        };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Update a member's role in an organization
    /// </summary>
    /// <remarks>
    /// Only organization admins can change roles. The last admin cannot be demoted.
    /// </remarks>
    /// <param name="orgId">The organization ID</param>
    /// <param name="membershipId">The membership ID of the member to update</param>
    /// <param name="command">The new role details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated member details</returns>
    /// <response code="200">Role updated successfully</response>
    /// <response code="400">Cannot demote the last admin</response>
    /// <response code="403">User is not an admin of this organization</response>
    /// <response code="404">Organization or membership not found</response>
    [HttpPut("{orgId:guid}/members/{membershipId:guid}/role")]
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
        command = command with
        {
            OrgId = orgId,
            UserId = userId,
            TargetMembershipId = membershipId
        };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }
}
