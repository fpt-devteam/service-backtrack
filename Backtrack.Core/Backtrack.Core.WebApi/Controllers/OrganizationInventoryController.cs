using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Organizations.CreateInventoryItem;
using Backtrack.Core.Application.Usecases.Organizations.DeleteInventoryItem;
using Backtrack.Core.Application.Usecases.Organizations.GetInventoryItemById;
using Backtrack.Core.Application.Usecases.Organizations.GetInventoryItems;
using Backtrack.Core.Application.Usecases.Organizations.SearchInventoryBySemantic;
using Backtrack.Core.Application.Usecases.Organizations.UpdateInventoryItem;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("orgs/{orgId:guid}/inventory")]
[Produces("application/json")]
public class OrganizationInventoryController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrganizationInventoryResult>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateInventoryItemAsync(
        [FromRoute] Guid orgId,
        [FromBody] CreateInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { OrgId = orgId, UserId = userId };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpGet]
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
            OrgId = orgId, UserId = userId,
            Page = page, PageSize = pageSize,
            SearchTerm = searchTerm, Status = status
        };
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(PagedResponse<OrganizationInventoryResult>.Create(result.Items, page, pageSize, result.Total));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationInventoryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryItemByIdAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetInventoryItemByIdQuery { Id = id, OrgId = orgId, UserId = userId };
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrganizationInventoryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInventoryItemAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid id,
        [FromBody] UpdateInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { Id = id, OrgId = orgId, UserId = userId };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteInventoryItemAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var command = new DeleteInventoryItemCommand { Id = id, OrgId = orgId, UserId = userId };
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("search/semantic")]
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
            OrgId = orgId, UserId = userId,
            SearchText = searchText,
            Page = page, PageSize = pageSize
        };
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(PagedResponse<InventorySemanticSearchResult>.Create(result.Items, page, pageSize, result.Total));
    }
}
