using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class PostMatchConfiguration : IEntityTypeConfiguration<PostMatch>
{
    public void Configure(EntityTypeBuilder<PostMatch> builder)
    {
        builder.ToTable("post_matches");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(pm => pm.LostPostId)
            .HasColumnName("lost_post_id")
            .IsRequired();

        builder.Property(pm => pm.FoundPostId)
            .HasColumnName("found_post_id")
            .IsRequired();

        builder.Property(pm => pm.MatchScore)
            .HasColumnName("match_score")
            .IsRequired();

        builder.Property(pm => pm.LocationScore)
            .HasColumnName("location_score")
            .IsRequired();

        builder.Property(pm => pm.DescriptionScore)
            .HasColumnName("description_score")
            .IsRequired();

        builder.Property(pm => pm.DistanceMeters)
            .HasColumnName("distance_meters")
            .IsRequired();

        builder.Property(pm => pm.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(pm => pm.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(pm => pm.DeletedAt)
            .HasColumnName("deleted_at");

        // Relationships
        builder.HasOne(pm => pm.LostPost)
            .WithMany()
            .HasForeignKey(pm => pm.LostPostId)
            .HasConstraintName("fk_post_matches_lost_post_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pm => pm.FoundPost)
            .WithMany()
            .HasForeignKey(pm => pm.FoundPostId)
            .HasConstraintName("fk_post_matches_found_post_id")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pm => pm.LostPostId)
            .HasDatabaseName("ix_post_matches_lost_post_id");

        builder.HasIndex(pm => pm.FoundPostId)
            .HasDatabaseName("ix_post_matches_found_post_id");

        // Unique index for active matches
        builder.HasIndex(pm => new { pm.FoundPostId, pm.LostPostId })
            .IsUnique()
            .HasDatabaseName("ux_post_matches_found_lost_active")
            .HasFilter("deleted_at IS NULL");

        // Global query filter for soft delete
        builder.HasQueryFilter(pm => pm.DeletedAt == null);
    }
}
