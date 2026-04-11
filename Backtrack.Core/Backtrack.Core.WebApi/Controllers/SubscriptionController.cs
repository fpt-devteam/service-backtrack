using Backtrack.Core.Application.Usecases.Subscriptions;
using Backtrack.Core.Application.Usecases.Subscriptions.CancelSubscription;
using Backtrack.Core.Application.Usecases.Subscriptions.CreateSubscription;
using Backtrack.Core.Application.Usecases.Subscriptions.GetSubscription;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("subscriptions")]
[Produces("application/json")]
public class SubscriptionController(IMediator mediator) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionAsync(CancellationToken cancellationToken)
    {
        var subscriber = ResolveSubscriber();
        var result = await mediator.Send(new GetSubscriptionQuery { Subscriber = subscriber }, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSubscriptionAsync(
        [FromBody] CreateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var subscriber = ResolveSubscriber();
        command = command with { Subscriber = subscriber };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpDelete("me")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSubscriptionAsync(
        [FromBody] CancelSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var subscriber = ResolveSubscriber();
        command = command with { Subscriber = subscriber };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    private SubscriberContext ResolveSubscriber()
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var email = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthEmail);
        var name = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthName);
        var orgIdHeader = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.OrgId);

        if (!string.IsNullOrEmpty(orgIdHeader) && Guid.TryParse(orgIdHeader, out var orgId))
        {
            return new SubscriberContext
            {
                SubscriberType = SubscriberType.Organization,
                OrganizationId = orgId,
                Email = email,
                Name = name,
            };
        }

        return new SubscriberContext
        {
            SubscriberType = SubscriberType.User,
            UserId = userId,
            Email = email,
            Name = name,
        };
    }
}
