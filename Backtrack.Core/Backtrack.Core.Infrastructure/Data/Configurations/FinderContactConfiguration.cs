using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class FinderContactConfiguration : IEntityTypeConfiguration<FinderContact>
{
    public void Configure(EntityTypeBuilder<FinderContact> builder)
    {
        builder.ToTable("finder_contacts");

        builder.HasKey(fc => fc.Id);

        builder.Property(fc => fc.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(fc => fc.InventoryId)
            .HasColumnName("inventory_id")
            .IsRequired();

        builder.HasOne(fc => fc.Inventory)
            .WithOne(oi => oi.FinderContact)
            .HasForeignKey<FinderContact>(fc => fc.InventoryId)
            .HasConstraintName("fk_finder_contacts_inventory_id_org_inventories_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(fc => fc.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(fc => fc.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(fc => fc.Phone)
            .HasColumnName("phone")
            .HasMaxLength(50);

        builder.Property(fc => fc.NationalId)
            .HasColumnName("national_id")
            .HasMaxLength(50);

        builder.Property(fc => fc.OrgMemberId)
            .HasColumnName("org_member_id")
            .HasMaxLength(100);

        builder.Property(fc => fc.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(fc => fc.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(fc => fc.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasIndex(fc => fc.InventoryId)
            .IsUnique()
            .HasDatabaseName("ix_finder_contacts_inventory_id");

        builder.HasQueryFilter(fc => fc.DeletedAt == null);
    }
}
