using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Infrastructure.Data.Configurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("organizations");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(o => o.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(o => o.Slug)
                .HasColumnName("slug")
                .HasMaxLength(255)
                .IsRequired();

            builder.HasIndex(o => o.Slug)
                .IsUnique()
                .HasDatabaseName("ix_organizations_slug");

            builder.Property(o => o.Address)
                .HasColumnName("address")
                .HasMaxLength(500);

            builder.Property(o => o.Phone)
                .HasColumnName("phone")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(o => o.IndustryType)
                .HasColumnName("industry_type")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(o => o.TaxIdentificationNumber)
                .HasColumnName("tax_identification_number")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(o => o.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(o => o.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(o => o.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasQueryFilter(o => o.DeletedAt == null);
        }
    }
}
