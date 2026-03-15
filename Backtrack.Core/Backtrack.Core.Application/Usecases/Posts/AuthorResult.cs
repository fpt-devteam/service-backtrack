using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Usecases.Posts;

public sealed record AuthorResult
{
    public required string Id { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
}

public static class AuthorResultMapper
{
    public static AuthorResult ToAuthorResult(this User user)
    {
        return new AuthorResult
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl
        };
    }
}
