using Backtrack.Core.Application.Usecases.Devices.RegisterDevice;
using Backtrack.Core.Application.Usecases.Devices.UnregisterDevice;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("devices")]
public class DevicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DevicesController> logger;

    public DevicesController(IMediator mediator, ILogger<DevicesController> logger)
    {
        _mediator = mediator;
        this.logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterDeviceAsync([FromBody] RegisterDeviceCommand command, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { UserId = userId };
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpPost("unregister")]
    public async Task<IActionResult> UnregisterDeviceAsync([FromBody] UnregisterDeviceCommand command, CancellationToken cancellationToken)
    {
        var userId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { UserId = userId };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
