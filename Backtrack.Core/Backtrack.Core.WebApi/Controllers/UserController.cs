using Microsoft.AspNetCore.Mvc;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Backtrack.Core.Application.Usecases.Users.Commands.EnsureUserExist;
using Backtrack.Core.Application.Usecases.Users.Queries.GetMe;

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
        var encodedAvatarUrl = Request.Headers[HeaderNames.AuthAvatarUrl].ToString();

        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException($"Required header '{HeaderNames.AuthId}' is missing. This indicates a configuration issue with the API Gateway or middleware.");

        var displayName = Base64Util.DecodeToUtf8(encodedDisplayName);
        var avatarUrl = Base64Util.DecodeToUtf8(encodedAvatarUrl);

        var command = new EnsureUserExistCommand
        {
            UserId = userId,
            Email = email,
            DisplayName = displayName,
            AvatarUrl = avatarUrl
        };

        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMeAsync(CancellationToken cancellationToken)
    {
        var userId = Request.Headers[HeaderNames.AuthId].ToString();

        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException($"Required header '{HeaderNames.AuthId}' is missing. This indicates a configuration issue with the API Gateway or middleware.");

        var query = new GetMeQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }
}
