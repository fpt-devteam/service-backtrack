namespace Backtrack.Core.Domain.Entities;

public sealed class PostImage : Entity<Guid>
{
    public required Guid PostId { get; set; }
    public required string Url { get; set; }
    public required string Base64Data { get; set; }
    public required string MimeType { get; set; }
    public string? FileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public required int DisplayOrder { get; set; }

    public Post Post { get; set; } = default!;
}
