using System.Text.Json;
using System.Text.Json.Serialization;
using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class PostMatchConfiguration : IEntityTypeConfiguration<PostMatch>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public void Configure(EntityTypeBuilder<PostMatch> builder)
    {
        builder.ToTable("post_matches");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.LostPostId).HasColumnName("lost_post_id").IsRequired();
        builder.Property(m => m.FoundPostId).HasColumnName("found_post_id").IsRequired();
        builder.Property(m => m.Score).HasColumnName("score").IsRequired();

        builder.Property(m => m.Evidence)
            .HasColumnName("evidence")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<MatchEvidence>>(v, JsonOptions) ?? new List<MatchEvidence>())
            .Metadata.SetValueComparer(new ValueComparer<List<MatchEvidence>>(
                (a, b) => JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
                v => JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),
                v => JsonSerializer.Deserialize<List<MatchEvidence>>(JsonSerializer.Serialize(v, JsonOptions), JsonOptions)!));

        builder.Property(m => m.Evidence).IsRequired();

        builder.Property(m => m.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");
        builder.Property(m => m.DeletedAt).HasColumnName("deleted_at");

        // Unique among active (non-deleted) records only — soft-deleted rows must not block re-insertion
        builder.HasIndex(m => new { m.LostPostId, m.FoundPostId })
            .HasDatabaseName("ix_post_matches_lost_found")
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        // Query by lost post, sorted by score
        builder.HasIndex(m => new { m.LostPostId, m.Status, m.Score })
            .HasDatabaseName("ix_post_matches_by_lost");

        // Query by found post, sorted by score
        builder.HasIndex(m => new { m.FoundPostId, m.Status, m.Score })
            .HasDatabaseName("ix_post_matches_by_found");

        builder.HasOne(m => m.LostPost)
            .WithMany()
            .HasForeignKey(m => m.LostPostId)
            .HasConstraintName("fk_post_matches_lost_post_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.FoundPost)
            .WithMany()
            .HasForeignKey(m => m.FoundPostId)
            .HasConstraintName("fk_post_matches_found_post_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(m => m.DeletedAt == null);
    }
}
