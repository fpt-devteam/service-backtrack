namespace Backtrack.Core.Domain.Entities;

public sealed class PostPersonalBelongingDetail
{
    public required Guid PostId { get; set; }

    public string? Color { get; set; }
    public string? Brand { get; set; }
    public string? Material { get; set; }
    public string? Size { get; set; }
    public string? Condition { get; set; }
    public string? DistinctiveMarks { get; set; }

    // AI-generated descriptions from images, concatenated
    public string? AiDescription { get; set; }

    // User-provided additional notes (contents of bag/wallet, context)
    public string? AdditionalDetails { get; set; }

    public string? ContentHash { get; set; }

    public Post Post { get; set; } = default!;
}
