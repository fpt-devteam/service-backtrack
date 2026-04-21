using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.OrganizationInventory.CreateInventoryItem;
using Backtrack.Core.Application.Usecases.OrganizationInventory.GetInventoryItemById;
using Backtrack.Core.Application.Usecases.OrganizationInventory.PublishInventoryItem;
using Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;
using Backtrack.Core.Application.Usecases.OrganizationInventory.UpdateInventoryItem;
using Backtrack.Core.Application.Usecases.Posts.DeletePost;
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
    [ProducesResponseType(typeof(ApiResponse<InventoryItemResult>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateInventoryItemAsync(
        [FromRoute] Guid orgId,
        [FromBody] CreateInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { StaffId = userId, OrgId = orgId };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InventoryItemResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryItemsAsync(
        [FromRoute] Guid orgId,
        [FromBody] SearchInventoryItemsCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { OrgId = orgId, UserId = userId };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPost("me/search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InventoryItemResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyInventoryItemsAsync(
        [FromRoute] Guid orgId,
        [FromBody] SearchInventoryItemsCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with
        {
            OrgId = orgId,
            UserId = userId,
            Filters = (command.Filters ?? new InventoryFilter()) with { StaffId = userId }
        };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryItemByIdAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetInventoryItemByIdQuery { PostId = id, UserId = userId, OrgId = orgId };
        var result = await mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInventoryItemAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid id,
        [FromBody] UpdateInventoryItemCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { PostId = id, UserId = userId, OrgId = orgId };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PublishInventoryItemAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var command = new PublishInventoryItemCommand { PostId = id, UserId = userId, OrgId = orgId };
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
        var command = new DeletePostCommand { PostId = id, UserId = userId };
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
