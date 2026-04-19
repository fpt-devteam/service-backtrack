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
        builder.Property(m => m.SourcePostId).HasColumnName("source_post_id").IsRequired();
        builder.Property(m => m.CandidatePostId).HasColumnName("candidate_post_id").IsRequired();
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
            .HasDefaultValue(MatchStatus.Pending)
            .IsRequired();

        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");
        builder.Property(m => m.DeletedAt).HasColumnName("deleted_at");

        // Unique: one candidate can only match one source once
        builder.HasIndex(m => new { m.SourcePostId, m.CandidatePostId })
            .HasDatabaseName("ix_post_matches_source_candidate")
            .IsUnique();

        // Query by source, sorted by score
        builder.HasIndex(m => new { m.SourcePostId, m.Status, m.Score })
            .HasDatabaseName("ix_post_matches_by_source");

        builder.HasOne(m => m.SourcePost)
            .WithMany()
            .HasForeignKey(m => m.SourcePostId)
            .HasConstraintName("fk_post_matches_source_post_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.CandidatePost)
            .WithMany()
            .HasForeignKey(m => m.CandidatePostId)
            .HasConstraintName("fk_post_matches_candidate_post_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(m => m.DeletedAt == null);
    }
}
