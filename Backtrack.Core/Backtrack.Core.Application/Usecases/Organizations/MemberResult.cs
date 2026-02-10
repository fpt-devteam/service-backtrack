namespace Backtrack.Core.Application.Usecases.Organizations;

public sealed record MemberResult
{
    public required Guid MembershipId { get; init; }
    public required string UserId { get; init; }
    public string? DisplayName { get; init; }
    public string? Email { get; init; }
    public string? AvatarUrl { get; init; }
    public required string Role { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset JoinedAt { get; init; }
}
