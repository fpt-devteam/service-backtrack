using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(d => d.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
        builder.Property(d => d.DeviceId).HasColumnName("device_id").HasMaxLength(255).IsRequired();
        builder.Property(d => d.IsActive).HasColumnName("is_active");
        builder.Property(d => d.LastSeenAt).HasColumnName("last_seen_at");
        builder.Property(d => d.CreatedAt).HasColumnName("created_at");
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");
        builder.Property(d => d.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("fk_devices_user_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(d => d.DeletedAt == null);

        builder.HasIndex(d => new { d.UserId, d.DeviceId })
            .IsUnique()
            .HasDatabaseName("ix_devices_user_id_device_id");

        builder.HasIndex(d => d.Token)
            .HasDatabaseName("ix_devices_token");
    }
}
