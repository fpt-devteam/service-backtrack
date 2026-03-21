using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Usecases.PostImages;

public sealed record PostImageResult
{
    public required Guid Id { get; init; }
    public required string Url { get; init; }
    public required int DisplayOrder { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public static class PostImageMapper
{
    public static PostImageResult ToPostImageResult(this PostImage image)
    {
        return new PostImageResult
        {
            Id = image.Id,
            Url = image.Url,
            DisplayOrder = image.DisplayOrder,
            CreatedAt = image.CreatedAt,
        };
    }
}
