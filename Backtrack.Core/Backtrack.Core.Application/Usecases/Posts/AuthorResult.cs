namespace Backtrack.Core.Application.Usecases.Posts;

public sealed record AuthorResult
{
    public required string Id { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
}
