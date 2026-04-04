using System.Text.Json;
using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class OrgHandoverConfiguration : IEntityTypeConfiguration<OrgHandover>
{
    public void Configure(EntityTypeBuilder<OrgHandover> builder)
    {
        builder.ToTable("org_handovers");

        builder.Property(h => h.FinderId)
            .HasColumnName("finder_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(h => h.OrgId)
            .HasColumnName("org_id")
            .IsRequired();

        builder.Property(h => h.StaffId)
            .HasColumnName("staff_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(h => h.FinderPostId)
            .HasColumnName("finder_post_id");

        builder.Property(h => h.OwnerVerified)
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

        builder.Property(h => h.OwnerFormData)
            .HasColumnName("owner_form_data")
            .HasColumnType("jsonb")
            .HasConversion(ownerFormDataConverter, ownerFormDataComparer);

        builder.Property(h => h.StaffConfirmedAt)
            .HasColumnName("staff_confirmed_at");

        builder.Property(h => h.OwnerConfirmedAt)
            .HasColumnName("owner_confirmed_at");

        builder.HasOne(h => h.Finder)
            .WithMany()
            .HasForeignKey(h => h.FinderId)
            .HasConstraintName("fk_org_handovers_finder_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Organization)
            .WithMany()
            .HasForeignKey(h => h.OrgId)
            .HasConstraintName("fk_org_handovers_org_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Staff)
            .WithMany()
            .HasForeignKey(h => h.StaffId)
            .HasConstraintName("fk_org_handovers_staff_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.FinderPost)
            .WithMany()
            .HasForeignKey(h => h.FinderPostId)
            .HasConstraintName("fk_org_handovers_finder_post_id")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(h => h.FinderId)
            .HasDatabaseName("ix_org_handovers_finder_id");

        builder.HasIndex(h => h.OrgId)
            .HasDatabaseName("ix_org_handovers_org_id");

        builder.HasIndex(h => h.StaffId)
            .HasDatabaseName("ix_org_handovers_staff_id");

        builder.HasIndex(h => h.FinderPostId)
            .HasDatabaseName("ix_org_handovers_finder_post_id");
    }
}
