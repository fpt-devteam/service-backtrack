namespace Backtrack.Core.Application.Users.Common;

public sealed record UserResult
{
    public required string Id { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
}
