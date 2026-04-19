namespace Backtrack.Core.Domain.Entities;

public sealed class PostOtherDetail
{
    public required Guid PostId { get; set; }

    // Required free-text identifier (what IS this thing?)
    public required string ItemIdentifier { get; set; }  // "Sapiens book", "hugging pillow"

    public string? PrimaryColor { get; set; }
    public string? AdditionalDetails { get; set; }

    public string? AiDescription { get; set; }

    public string? ContentHash { get; set; }

    public Post Post { get; set; } = default!;
}
