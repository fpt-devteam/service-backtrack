using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Application.Usecases.Subscriptions;
using Backtrack.Core.Application.Usecases.Subscriptions.CancelSubscription;
using Backtrack.Core.Application.Usecases.Subscriptions.CreateSubscription;
using Backtrack.Core.Application.Usecases.Subscriptions.GetPaymentHistories;
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
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var subscriber = new SubscriberContext
        {
            SubscriberType = command.OrganizationId.HasValue ? SubscriberType.Organization : SubscriberType.User,
            UserId = userId,
            OrganizationId = command.OrganizationId,
        };
        command = command with { Subscriber = subscriber };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpGet("payments")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentHistoryResult>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPaymentHistoriesAsync(
        [FromQuery] Guid? orgId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var subscriber = new SubscriberContext
        {
            SubscriberType = orgId.HasValue ? SubscriberType.Organization : SubscriberType.User,
            UserId = userId,
            OrganizationId = orgId,
        };
        var result = await mediator.Send(new GetPaymentHistoriesQuery
        {
            Subscriber = subscriber,
            Page = page,
            PageSize = pageSize,
        }, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpDelete("me")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSubscriptionAsync(
        [FromBody] CancelSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var subscriber = new SubscriberContext
        {
            SubscriberType = command.OrganizationId.HasValue ? SubscriberType.Organization : SubscriberType.User,
            UserId = userId,
            OrganizationId = command.OrganizationId,
        };
        command = command with { Subscriber = subscriber };
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Resolves subscriber context for endpoints that have no request body (GET).
    /// orgId comes from the X-Organization-Id header set by the gateway.
    /// UserId is always included so handlers know the caller identity.
    /// </summary>
    private SubscriberContext ResolveSubscriber()
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var orgIdHeader = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.OrgId);

        if (!string.IsNullOrEmpty(orgIdHeader) && Guid.TryParse(orgIdHeader, out var orgId))
        {
            // orgId present → org subscription; look up by orgId, not by userId
            return new SubscriberContext
            {
                SubscriberType = SubscriberType.Organization,
                UserId = userId,
                OrganizationId = orgId,
            };
        }

        // no orgId → user subscription; look up by userId
        return new SubscriberContext
        {
            SubscriberType = SubscriberType.User,
            UserId = userId,
        };
    }
}
