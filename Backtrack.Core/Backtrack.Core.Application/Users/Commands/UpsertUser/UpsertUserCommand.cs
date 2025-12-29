using Backtrack.Core.Application.Users.Common;
using MediatR;

namespace Backtrack.Core.Application.Users.Commands.UpsertUser;

public sealed record UpsertUserCommand : IRequest<UserResult>
{
    public required string UserId { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
}
