using System.Text.Json;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class OrgReturnReportConfiguration : IEntityTypeConfiguration<OrgReturnReport>
{
    public void Configure(EntityTypeBuilder<OrgReturnReport> builder)
    {
        builder.ToTable("org_return_reports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id");

        builder.Property(r => r.OrgId)
            .HasColumnName("org_id")
            .IsRequired();

        builder.Property(r => r.StaffId)
            .HasColumnName("staff_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(r => r.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(r => r.DeletedAt)
            .HasColumnName("deleted_at");

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var ownerInfoConverter = new ValueConverter<OwnerInfo?, string?>(
            toDb => toDb == null ? null : JsonSerializer.Serialize(toDb, jsonOptions),
            fromDb => fromDb == null ? null : JsonSerializer.Deserialize<OwnerInfo>(fromDb, jsonOptions)
        );

        builder.Property(r => r.OwnerInfo)
            .HasColumnName("owner_info")
            .HasColumnType("jsonb")
            .HasConversion(ownerInfoConverter);

        builder.HasOne(r => r.Organization)
            .WithMany()
            .HasForeignKey(r => r.OrgId)
            .HasConstraintName("fk_org_return_reports_org_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Staff)
            .WithMany()
            .HasForeignKey(r => r.StaffId)
            .HasConstraintName("fk_org_return_reports_staff_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Post)
            .WithMany()
            .HasForeignKey(r => r.PostId)
            .HasConstraintName("fk_org_return_reports_post_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(r => r.DeletedAt == null);

        builder.HasIndex(r => r.OrgId)
            .HasDatabaseName("ix_org_return_reports_org_id");

        builder.HasIndex(r => r.StaffId)
            .HasDatabaseName("ix_org_return_reports_staff_id");

        builder.HasIndex(r => r.PostId)
            .HasDatabaseName("ix_org_return_reports_post_id");
    }
}
