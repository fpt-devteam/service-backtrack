using Microsoft.AspNetCore.Mvc;
using MediatR;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Utils;
using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Organizations.UpdateMemberRole;
using Backtrack.Core.Application.Usecases.Organizations.CreateOrganization;
using Backtrack.Core.Application.Usecases.Organizations.UpdateOrganization;
using Backtrack.Core.Application.Usecases.Organizations.GetOrganization;
using Backtrack.Core.Application.Usecases.Organizations.GetOrganizationPublic;
using Backtrack.Core.Application.Usecases.Organizations.GetMyOrganizations;
using Backtrack.Core.Application.Usecases.Organizations.GetOrgMembers;
using Backtrack.Core.Application.Usecases.Organizations.RemoveMember;
using Backtrack.Core.Application.Usecases.Organizations.CreateInventoryItem;
using Backtrack.Core.Application.Usecases.Organizations.UpdateInventoryItem;
using Backtrack.Core.Application.Usecases.Organizations.DeleteInventoryItem;
using Backtrack.Core.Application.Usecases.Organizations.GetInventoryItems;
using Backtrack.Core.Application.Usecases.Organizations.GetInventoryItemById;
using Backtrack.Core.Application.Usecases.Organizations.SearchInventoryBySemantic;
using Backtrack.Core.Application.Usecases.Organizations.GetAllOrganizations;
using Backtrack.Core.Application.Usecases.Organizations.GetSettingOrganizationById;

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
    /// Get all organizations registered in Backtrack
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of all organizations</returns>
    /// <response code="200">Returns all organizations</response>
    [HttpGet("public")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<OrganizationResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrganizationsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllOrganizationsQuery(page, pageSize);
        var (items, total) = await _mediator.Send(query, cancellationToken);
        var response = PagedResponse<OrganizationResult>.Create(items, page, pageSize, total);
        return this.ApiOk(response);
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
    /// Get an organization by ID (public, no authentication or membership required)
    /// </summary>
    /// <param name="orgId">The organization ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The organization details</returns>
    /// <response code="200">Organization found</response>
    /// <response code="404">Organization not found</response>
    [HttpGet("{orgId:guid}/public")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationPublicAsync(
        [FromRoute] Guid orgId, CancellationToken cancellationToken)
    {
        var query = new GetOrganizationPublicQuery(orgId);
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Get organization settings by ID (public, no authentication required)
    /// </summary>
    /// <param name="orgId">The organization ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The organization settings containing required finder contact fields</returns>
    /// <response code="200">Organization settings found</response>
    /// <response code="404">Organization not found</response>
    [HttpGet("{orgId:guid}/settings/public")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationSettingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSettingOrganizationByIdAsync(
        [FromRoute] Guid orgId, CancellationToken cancellationToken)
    {
        var query = new GetSettingOrganizationByIdQuery(orgId);
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

    /// <summary>
    /// Add an item to organization inventory
    /// </summary>
    [HttpPost("{orgId:guid}/inventory")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationInventoryResult>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateInventoryItemAsync(
        [FromRoute] Guid orgId,
        [FromBody] CreateInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { OrgId = orgId, UserId = userId };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    /// <summary>
    /// Get organization inventory items
    /// </summary>
    [HttpGet("{orgId:guid}/inventory")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<OrganizationInventoryResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryItemsAsync(
        [FromRoute] Guid orgId,
        [FromQuery] string? searchTerm,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetInventoryItemsQuery
        {
            OrgId = orgId,
            UserId = userId,
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            Status = status
        };
        var result = await _mediator.Send(query, cancellationToken);
        var response = PagedResponse<OrganizationInventoryResult>.Create(result.Items, page, pageSize, result.Total);
        return this.ApiOk(response);
    }

    /// <summary>
    /// Get organization inventory item by ID
    /// </summary>
    [HttpGet("{orgId:guid}/inventory/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationInventoryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryItemByIdAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetInventoryItemByIdQuery { Id = id, OrgId = orgId, UserId = userId };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Update an organization inventory item
    /// </summary>
    [HttpPut("{orgId:guid}/inventory/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationInventoryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInventoryItemAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid id,
        [FromBody] UpdateInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { Id = id, OrgId = orgId, UserId = userId };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Delete an organization inventory item
    /// </summary>
    [HttpDelete("{orgId:guid}/inventory/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteInventoryItemAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var command = new DeleteInventoryItemCommand { Id = id, OrgId = orgId, UserId = userId };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Search organization inventory items by semantic similarity
    /// </summary>
    [HttpGet("{orgId:guid}/inventory/search/semantic")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<InventorySemanticSearchResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchInventoryBySemanticAsync(
        [FromRoute] Guid orgId,
        [FromQuery] string searchText,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new SearchInventoryBySemanticQuery
        {
            OrgId = orgId,
            UserId = userId,
            SearchText = searchText,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        var response = PagedResponse<InventorySemanticSearchResult>.Create(result.Items, page, pageSize, result.Total);
        return this.ApiOk(response);
    }
}
