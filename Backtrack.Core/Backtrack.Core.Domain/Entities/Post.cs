using Backtrack.Core.Domain.Common;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using System.Security.Cryptography;
using System.Text;

namespace Backtrack.Core.Domain.Entities
{
    public sealed class Post : Entity<Guid>
    {
        public required string AuthorId { get; set; }
        public required PostType PostType { get; set; }
        public required string ItemName { get; set; }
        public required string Description { get; set; }
        public string? DistinctiveMarks { get; set; }
        public string[] ImageUrls { get; set; } = Array.Empty<string>();
        public GeoPoint? Location { get; set; }
        public string? ExternalPlaceId { get; set; }
        public string? DisplayAddress { get; set; }
        public float[]? ContentEmbedding { get; set; }
        public required ContentEmbeddingStatus ContentEmbeddingStatus { get; set; }
        public required string ContentHash { get; set; }
        public required DateTimeOffset EventTime { get; set; }

        public User Author { get; set; } = default!;
    }
}
