using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Pgvector;

namespace Backtrack.Core.Infrastructure.Data.Configurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("posts");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(p => p.PostType)
                .HasColumnName("post_type")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.ItemName)
                .HasColumnName("item_name")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasColumnName("description")
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(p => p.ImageUrls)
                .HasColumnName("image_urls")
                .HasColumnType("text[]")
                .IsRequired();

            var geoPointToPointConverter = new ValueConverter<GeoPoint?, Point?>(
                toDb => toDb == null
                    ? null
                    : new Point(toDb.Longitude, toDb.Latitude) { SRID = 4326 },
                fromDb => fromDb == null
                    ? null
                    : new GeoPoint(fromDb.Y, fromDb.X)
            );

            var geoPointComparer = new ValueComparer<GeoPoint?>(
                (a, b) => a == null && b == null
                          || (a != null && b != null && a.Latitude == b.Latitude && a.Longitude == b.Longitude),
                v => v == null ? 0 : HashCode.Combine(v.Latitude, v.Longitude),
                v => v == null ? null : new GeoPoint(v.Latitude, v.Longitude)
            );

            builder.Property(p => p.Location)
                .HasColumnName("location")
                .HasColumnType("geography(point, 4326)")
                .HasConversion(geoPointToPointConverter, geoPointComparer);

            builder.Property(p => p.ExternalPlaceId)
                .HasColumnName("external_place_id")
                .HasMaxLength(500);

            builder.Property(p => p.DisplayAddress)
                .HasColumnName("display_address")
                .HasMaxLength(1000);

            // Vector converter for embeddings
            var embeddingToVectorConverter = new ValueConverter<float[]?, Vector?>(
                toDb => toDb == null ? null : new Vector(toDb),
                fromDb => fromDb == null ? null : fromDb.ToArray()
            );

            var embeddingComparer = new ValueComparer<float[]?>(
                (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                v => v == null ? 0 : v.GetHashCode(),
                v => v == null ? null : v.ToArray()
            );

            builder.Property(p => p.ContentEmbedding)
                .HasColumnName("content_embedding")
                .HasColumnType("vector(768)")
                .HasConversion(embeddingToVectorConverter, embeddingComparer);

            builder.Property(p => p.ContentHash)
                .HasColumnName("content_hash")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(p => p.ContentEmbeddingStatus)
                .HasColumnName("content_embedding_status")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.EventTime)
                .HasColumnName("event_time")
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(p => p.DeletedAt)
                .HasColumnName("deleted_at");

            // Indexes
            builder.HasIndex(p => p.PostType)
                .HasDatabaseName("ix_posts_post_type");

            builder.HasIndex(p => p.EventTime)
                .HasDatabaseName("ix_posts_event_time");

            // Spatial index for location using GIST
            builder.HasIndex(p => p.Location)
                .HasDatabaseName("ix_posts_location")
                .HasMethod("gist");

            // Vector index for content embedding using HNSW for efficient similarity search
            builder.HasIndex(p => p.ContentEmbedding)
                .HasDatabaseName("ix_posts_content_embedding")
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");

            // Global query filter for soft delete
            builder.HasQueryFilter(p => p.DeletedAt == null);
        }
    }
}
