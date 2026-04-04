using Backtrack.Core.Application.Usecases.Dev.JoinOrganization;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("dev")]
public class DevController(IMediator mediator, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("join-organization")]
    public async Task<IActionResult> JoinOrganizationAsync(
        [FromBody] DevJoinOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        if (!env.IsDevelopment())
            return NotFound();

        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { UserId = userId };

        var result = await mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }
}
