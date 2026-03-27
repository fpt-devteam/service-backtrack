using System.Text.Json;
using System.Text.Json.Serialization;
using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class HandoverOrgExtensionConfiguration : IEntityTypeConfiguration<HandoverOrgExtension>
{
    public void Configure(EntityTypeBuilder<HandoverOrgExtension> builder)
    {
        builder.ToTable("handover_org_extensions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.HandoverId)
            .HasColumnName("handover_id")
            .IsRequired();

        builder.Property(e => e.OrgId)
            .HasColumnName("org_id")
            .IsRequired();

        builder.Property(e => e.StaffId)
            .HasColumnName("staff_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.OwnerVerified)
            .HasColumnName("owner_verified")
            .HasDefaultValue(false)
            .IsRequired();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var ownerFormDataConverter = new ValueConverter<Dictionary<string, string>?, string?>(
            toDb => toDb == null ? null : JsonSerializer.Serialize(toDb, jsonOptions),
            fromDb => fromDb == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(fromDb, jsonOptions)
        );

        var ownerFormDataComparer = new ValueComparer<Dictionary<string, string>?>(
            (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
            v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
            v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)
        );

        builder.Property(e => e.OwnerFormData)
            .HasColumnName("owner_form_data")
            .HasColumnType("jsonb")
            .HasConversion(ownerFormDataConverter, ownerFormDataComparer);

        builder.Property(e => e.StaffConfirmedAt)
            .HasColumnName("staff_confirmed_at");

        builder.Property(e => e.OwnerConfirmedAt)
            .HasColumnName("owner_confirmed_at");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(e => e.DeletedAt)
            .HasColumnName("deleted_at");

        // Relationships
        builder.HasOne(e => e.Organization)
            .WithMany()
            .HasForeignKey(e => e.OrgId)
            .HasConstraintName("fk_handover_org_extensions_org_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Staff)
            .WithMany()
            .HasForeignKey(e => e.StaffId)
            .HasConstraintName("fk_handover_org_extensions_staff_id")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.HandoverId)
            .IsUnique()
            .HasDatabaseName("ix_handover_org_extensions_handover_id");

        builder.HasIndex(e => e.OrgId)
            .HasDatabaseName("ix_handover_org_extensions_org_id");

        builder.HasIndex(e => e.StaffId)
            .HasDatabaseName("ix_handover_org_extensions_staff_id");

        // Global query filter for soft delete
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}
