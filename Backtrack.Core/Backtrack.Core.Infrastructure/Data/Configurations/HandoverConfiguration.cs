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

        // Indexes
        builder.HasIndex(h => h.Status)
            .HasDatabaseName("ix_handovers_status");

        builder.HasIndex(h => h.ExpiresAt)
            .HasDatabaseName("ix_handovers_expires_at");

        // Global query filter for soft delete
        builder.HasQueryFilter(h => h.DeletedAt == null);
    }
}
