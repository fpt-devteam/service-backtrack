using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class HandoverConfiguration : IEntityTypeConfiguration<Handover>
{
    public void Configure(EntityTypeBuilder<Handover> builder)
    {
        builder.ToTable("handovers");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(h => h.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(h => h.FinderPostId)
            .HasColumnName("finder_post_id");

        builder.Property(h => h.OwnerPostId)
            .HasColumnName("owner_post_id");

        builder.Property(h => h.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(h => h.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.Property(h => h.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(h => h.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(h => h.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(h => h.DeletedAt)
            .HasColumnName("deleted_at");

        // Relationships
        builder.HasOne(h => h.FinderPost)
            .WithMany()
            .HasForeignKey(h => h.FinderPostId)
            .HasConstraintName("fk_handovers_finder_post_id")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(h => h.OwnerPost)
            .WithMany()
            .HasForeignKey(h => h.OwnerPostId)
            .HasConstraintName("fk_handovers_owner_post_id")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(h => h.OrgExtension)
            .WithOne(e => e.Handover)
            .HasForeignKey<HandoverOrgExtension>(e => e.HandoverId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(h => h.FinderPostId)
            .HasDatabaseName("ix_handovers_finder_post_id");

        builder.HasIndex(h => h.OwnerPostId)
            .HasDatabaseName("ix_handovers_owner_post_id");

        builder.HasIndex(h => h.Status)
            .HasDatabaseName("ix_handovers_status");

        builder.HasIndex(h => h.ExpiresAt)
            .HasDatabaseName("ix_handovers_expires_at");

        // Global query filter for soft delete
        builder.HasQueryFilter(h => h.DeletedAt == null);
    }
}
