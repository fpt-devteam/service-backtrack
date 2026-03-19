using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Usecases.PostImages;

public sealed record PostImageResult
{
    public required Guid Id { get; init; }
    public required Guid PostId { get; init; }
    public required string Url { get; init; }
    public required string Base64Data { get; init; }
    public required string MimeType { get; init; }
    public string? FileName { get; init; }
    public long? FileSizeBytes { get; init; }
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
            PostId = image.PostId,
            Url = image.Url,
            Base64Data = image.Base64Data,
            MimeType = image.MimeType,
            FileName = image.FileName,
            FileSizeBytes = image.FileSizeBytes,
            DisplayOrder = image.DisplayOrder,
            CreatedAt = image.CreatedAt,
        };
    }
}
