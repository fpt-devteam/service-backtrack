using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Domain.Entities;

public sealed class Post : Entity<Guid>
{
    public required string AuthorId { get; set; }
    public required PostType PostType { get; set; }

    // Top-level classification (enum — drives matching strategy)
    public required ItemCategory Category { get; set; }

    // Reference to subcategory lookup table
    public required Guid SubcategoryId { get; set; }
    public Subcategory Subcategory { get; set; } = default!;

    // Location
    public required GeoPoint Location { get; set; }
    public required string DisplayAddress { get; set; }
    public string? ExternalPlaceId { get; set; }

    // Temporal
    public required DateTimeOffset EventTime { get; set; }

    // Images (URLs; AI describes them into detail tables)
    public List<string> ImageUrls { get; set; } = new();

    // Text embedding (single source of truth for matching)
    public float[]? Embedding { get; set; }
    public required EmbeddingStatus EmbeddingStatus { get; set; }
    public required PostMatchingStatus PostMatchingStatus { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Active;

    // Relationships
    public Guid? OrganizationId { get; set; }
    public User Author { get; set; } = default!;
    public Organization? Organization { get; set; } = null;

    // Category-specific details (exactly ONE is populated based on Category)
    public PostPersonalBelongingDetail? PersonalBelongingDetail { get; set; }
    public PostCardDetail? CardDetail { get; set; }
    public PostElectronicDetail? ElectronicDetail { get; set; }
    public PostOtherDetail? OtherDetail { get; set; }
}
