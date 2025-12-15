using Backtrack.Core.Application.Users.Common;
using MediatR;

namespace Backtrack.Core.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand : IRequest<UserResult>
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
}
