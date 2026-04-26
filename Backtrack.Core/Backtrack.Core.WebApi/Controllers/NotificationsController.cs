using Backtrack.Core.Application.Usecases.Notifications.GetUnreadCount;
using Backtrack.Core.Application.Usecases.Notifications.GetUserNotifications;
using Backtrack.Core.Application.Usecases.Notifications.SendPushNotification;
using Backtrack.Core.Application.Usecases.Notifications.UpdateNotificationStatus;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> SendPushNotificationAsync([FromBody] SendPushNotificationCommand command, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { UserId = userId };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserNotificationsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetUserNotificationsQuery { UserId = userId, Page = page, PageSize = pageSize };
        var (items, total) = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(PagedResponse<NotificationResult>.Create(items, page, pageSize, total));
    }

    [HttpPut("status")]
    public async Task<IActionResult> UpdateNotificationStatusAsync([FromBody] UpdateNotificationStatusCommand command, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { UserId = userId };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetUnreadCountQuery { UserId = userId };
        var count = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(new { Count = count });
    }
}
