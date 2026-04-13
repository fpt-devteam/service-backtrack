using Backtrack.Core.Application.Usecases.Dev.JoinOrganization;
using Backtrack.Core.Application.Usecases.Dev.SeedSubscriptionPlans;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("dev")]
public class DevController(IMediator mediator) : ControllerBase
{
    [HttpPost("join-organization")]
    public async Task<IActionResult> JoinOrganizationAsync(
        [FromBody] DevJoinOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { UserId = userId };

        var result = await mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpPost("seed-subscription-plans")]
    public async Task<IActionResult> SeedSubscriptionPlansAsync(
        [FromBody] SeedSubscriptionPlansCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }
}
