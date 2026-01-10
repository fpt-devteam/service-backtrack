using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.WebApi.Contracts.Users.Responses;

public sealed record UserResponse
{
    public required string Id { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public required UserGlobalRole GlobalRole { get; init; }
}
