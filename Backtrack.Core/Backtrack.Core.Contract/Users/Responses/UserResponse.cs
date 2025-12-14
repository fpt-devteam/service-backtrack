namespace Backtrack.Core.Contract.Users.Responses;

public sealed record UserResponse
{
    public required string Id { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
}
