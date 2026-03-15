namespace Backtrack.Core.Application.Usecases.Users.GetPublicUserProfile;

public sealed record PublicUserProfileResult
{
    public required string Id { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
}
