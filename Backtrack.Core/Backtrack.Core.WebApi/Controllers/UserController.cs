using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backtrack.Core.Application.Users.Commands.CreateUser;
using Backtrack.Core.Application.Users.Queries.GetMe;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Backtrack.Core.WebApi.Contracts.Users.Responses;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAsync(CancellationToken cancellationToken)
    {
        var userId = Request.Headers[HeaderNames.AuthId].ToString();
        var email = Request.Headers[HeaderNames.AuthEmail].ToString();
        var encodedDisplayName = Request.Headers[HeaderNames.AuthName].ToString();

        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException($"Required header '{HeaderNames.AuthId}' is missing. This indicates a configuration issue with the API Gateway or middleware.");

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException($"Required header '{HeaderNames.AuthEmail}' is missing. This indicates a configuration issue with the API Gateway or middleware.");

        var displayName = Base64Util.DecodeToUtf8(encodedDisplayName);

        var command = new CreateUserCommand
        {
            UserId = userId,
            Email = email,
            DisplayName = displayName
        };

        var result = await _mediator.Send(command, cancellationToken);

        var response = new UserResponse
        {
            Id = result.Id,
            Email = result.Email,
            DisplayName = result.DisplayName
        };

        return this.ApiCreated(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMeAsync(CancellationToken cancellationToken)
    {
        var userId = Request.Headers[HeaderNames.AuthId].ToString();

        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException($"Required header '{HeaderNames.AuthId}' is missing. This indicates a configuration issue with the API Gateway or middleware.");

        var query = new GetMeQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);

        var response = new UserResponse
        {
            Id = result.Id,
            Email = result.Email,
            DisplayName = result.DisplayName
        };

        return this.ApiOk(response);
    }
}
