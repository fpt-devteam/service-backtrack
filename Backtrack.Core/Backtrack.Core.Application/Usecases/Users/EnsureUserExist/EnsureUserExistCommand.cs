using MediatR;

namespace Backtrack.Core.Application.Usecases.Users.EnsureUserExist;

public sealed record EnsureUserExistCommand : IRequest<UserResult>
{
    public required string UserId { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
}
