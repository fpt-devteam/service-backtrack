using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.Users;

public sealed record UserResult
{
    public required string Id { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Phone { get; init; }
    public bool ShowEmail { get; init; }
    public bool ShowPhone { get; init; }
    public required UserGlobalRole GlobalRole { get; init; }
    public required UserStatus Status { get; init; }
}
