using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Users.Commands.CreateUser;
using Backtrack.Core.Application.Users.Queries.GetMe;
using Backtrack.Core.Contract.Users.Requests;
using Backtrack.Core.Contract.Users.Responses;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Extensions;
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
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand
        {
            UserId = request.UserId,
            Email = request.Email,
            DisplayName = request.DisplayName
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
