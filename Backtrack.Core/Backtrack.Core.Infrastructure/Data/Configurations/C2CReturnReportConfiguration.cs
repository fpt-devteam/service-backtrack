using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class C2CReturnReportConfiguration : IEntityTypeConfiguration<C2CReturnReport>
{
    public void Configure(EntityTypeBuilder<C2CReturnReport> builder)
    {
        builder.ToTable("c2c_return_reports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id");

        builder.Property(r => r.FinderId)
            .HasColumnName("finder_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(r => r.OwnerId)
            .HasColumnName("owner_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(r => r.FinderPostId)
            .HasColumnName("finder_post_id");

        builder.Property(r => r.OwnerPostId)
            .HasColumnName("owner_post_id");

        builder.Property(r => r.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(r => r.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasOne(r => r.Finder)
            .WithMany()
            .HasForeignKey(r => r.FinderId)
            .HasConstraintName("fk_c2c_return_reports_finder_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Owner)
            .WithMany()
            .HasForeignKey(r => r.OwnerId)
            .HasConstraintName("fk_c2c_return_reports_owner_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.FinderPost)
            .WithMany()
            .HasForeignKey(r => r.FinderPostId)
            .HasConstraintName("fk_c2c_return_reports_finder_post_id")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.OwnerPost)
            .WithMany()
            .HasForeignKey(r => r.OwnerPostId)
            .HasConstraintName("fk_c2c_return_reports_owner_post_id")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(r => r.DeletedAt == null);

        builder.HasIndex(r => r.FinderId)
            .HasDatabaseName("ix_c2c_return_reports_finder_id");

        builder.HasIndex(r => r.OwnerId)
            .HasDatabaseName("ix_c2c_return_reports_owner_id");

        builder.HasIndex(r => r.FinderPostId)
            .HasDatabaseName("ix_c2c_return_reports_finder_post_id");

        builder.HasIndex(r => r.OwnerPostId)
            .HasDatabaseName("ix_c2c_return_reports_owner_post_id");
    }
}
