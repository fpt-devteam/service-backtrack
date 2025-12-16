using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Users.Commands.CreateUser;
using Backtrack.Core.Application.Users.Queries.GetMe;
using Backtrack.Core.Contract.Users.Responses;
using Backtrack.Core.WebApi.Constants;
using MediatR;

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
        // Extract user information from headers
        var userId = Request.Headers[HeaderNames.AuthId].ToString();
        var email = Request.Headers[HeaderNames.AuthEmail].ToString();
        var encodedDisplayName = Request.Headers[HeaderNames.AuthName].ToString();

        // Validate required headers
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException(UserErrors.InvalidAuthId);

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException(UserErrors.InvalidEmail);

        // Decode display name from Base64
        var displayName = string.Empty;
        if (!string.IsNullOrWhiteSpace(encodedDisplayName))
        {
            try
            {
                var decodedBytes = Convert.FromBase64String(encodedDisplayName);
                displayName = System.Text.Encoding.UTF8.GetString(decodedBytes);
            }
            catch (FormatException)
            {
                throw new DomainException(UserErrors.InvalidDisplayName);
            }
        }

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
            throw new DomainException(UserErrors.NotFound);

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
