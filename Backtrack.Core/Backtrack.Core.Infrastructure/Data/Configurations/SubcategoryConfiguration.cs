using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class SubcategoryConfiguration : IEntityTypeConfiguration<Subcategory>
{
    public void Configure(EntityTypeBuilder<Subcategory> builder)
    {
        builder.ToTable("subcategories");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        builder.Property(s => s.DeletedAt).HasColumnName("deleted_at");

        // Unique per (category, code)
        builder.HasIndex(s => new { s.Category, s.Code })
            .HasDatabaseName("ix_subcategories_category_code")
            .IsUnique();

        builder.HasIndex(s => s.Category)
            .HasDatabaseName("ix_subcategories_category");

        builder.HasQueryFilter(s => s.DeletedAt == null);
    }
}
