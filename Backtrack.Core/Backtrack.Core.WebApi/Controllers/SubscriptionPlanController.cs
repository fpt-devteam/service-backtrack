using Backtrack.Core.Application.Usecases.SubscriptionPlans;
using Backtrack.Core.Application.Usecases.SubscriptionPlans.GetSubscriptionPlans;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("subscription-plans")]
[Produces("application/json")]
public class SubscriptionPlanController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SubscriptionPlanResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionPlansAsync(
        [FromQuery] SubscriberType subscriberType = SubscriberType.User,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetSubscriptionPlansQuery { SubscriberType = subscriberType }, cancellationToken);
        return this.ApiOk(result);
    }
}
