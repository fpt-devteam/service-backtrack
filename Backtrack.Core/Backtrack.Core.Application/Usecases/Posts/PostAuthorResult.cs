using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Usecases.Posts;

public sealed record PostAuthorResult
{
    public required string Id { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
}

public static class PostAuthorResultMapper
{
    public static PostAuthorResult ToPostAuthorResult(this User user)
    {
        return new PostAuthorResult
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl
        };
    }
}
