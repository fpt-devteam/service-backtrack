using Backtrack.Core.Application.Usecases.StripeWebhooks.HandleStripeEvent;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("webhooks/stripe")]
public class StripeWebhookController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HandleStripeEventAsync(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        await mediator.Send(new HandleStripeEventCommand { Json = json, Signature = signature }, cancellationToken);
        return Ok();
    }
}
