using System.Text.Json;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class OrgReceiveReportConfiguration : IEntityTypeConfiguration<OrgReceiveReport>
{
    public void Configure(EntityTypeBuilder<OrgReceiveReport> builder)
    {
        builder.ToTable("org_receive_reports");

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

        var finderInfoConverter = new ValueConverter<FinderInfo, string>(
            toDb => JsonSerializer.Serialize(toDb, jsonOptions),
            fromDb => JsonSerializer.Deserialize<FinderInfo>(fromDb, jsonOptions)!
        );

        builder.Property(r => r.FinderInfo)
            .HasColumnName("finder_info")
            .HasColumnType("jsonb")
            .IsRequired()
            .HasConversion(finderInfoConverter);

        builder.HasOne(r => r.Organization)
            .WithMany()
            .HasForeignKey(r => r.OrgId)
            .HasConstraintName("fk_org_receive_reports_org_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Staff)
            .WithMany()
            .HasForeignKey(r => r.StaffId)
            .HasConstraintName("fk_org_receive_reports_staff_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Post)
            .WithMany()
            .HasForeignKey(r => r.PostId)
            .HasConstraintName("fk_org_receive_reports_post_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(r => r.DeletedAt == null);

        builder.HasIndex(r => r.OrgId)
            .HasDatabaseName("ix_org_receive_reports_org_id");

        builder.HasIndex(r => r.StaffId)
            .HasDatabaseName("ix_org_receive_reports_staff_id");

        builder.HasIndex(r => r.PostId)
            .IsUnique()
            .HasDatabaseName("ix_org_receive_reports_post_id");
    }
}
