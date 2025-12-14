using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Users.Commands;
using Backtrack.Core.Application.Users.Queries;
using Backtrack.Core.Contract.Users.Requests;
using Backtrack.Core.Domain.Constants;
using MediatR;
using Backtrack.Core.Application.Users.Commands.CreateUser;
using Backtrack.Core.Application.Users.Queries.GetMe;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private const string AuthIdHeaderName = "X-Auth-Id";
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request);
        var response = await _mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMeAsync(CancellationToken cancellationToken)
    {
        var userId = Request.Headers[AuthIdHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException(UserErrors.NotFound);

        var query = new GetMeQuery(userId);
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
    }
}
