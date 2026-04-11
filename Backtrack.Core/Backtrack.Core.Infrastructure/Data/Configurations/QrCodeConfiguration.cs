using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class QrCodeConfiguration : IEntityTypeConfiguration<QrCode>
{
    public void Configure(EntityTypeBuilder<QrCode> builder)
    {
        builder.ToTable("qr_codes");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id).HasColumnName("id").IsRequired();
        builder.Property(q => q.UserId).HasColumnName("user_id").HasMaxLength(128).IsRequired();
        builder.Property(q => q.PublicCode).HasColumnName("public_code").HasMaxLength(13).IsRequired(); // BTK-XXXXXXXX
        builder.Property(q => q.Note).HasColumnName("note").HasMaxLength(500);
        builder.Property(q => q.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(q => q.UpdatedAt).HasColumnName("updated_at");
        builder.Property(q => q.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(q => q.UserId).HasDatabaseName("ix_qr_codes_user_id");
        builder.HasIndex(q => q.PublicCode).IsUnique().HasDatabaseName("ux_qr_codes_public_code");

        builder.HasQueryFilter(q => q.DeletedAt == null);
    }
}
