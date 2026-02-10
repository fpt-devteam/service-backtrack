using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Infrastructure.Data.Configurations
{
    public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
    {
        public void Configure(EntityTypeBuilder<Membership> builder)
        {
            builder.ToTable("memberships");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(m => m.OrganizationId)
                .HasColumnName("organization_id")
                .IsRequired();

            builder.Property(m => m.UserId)
                .HasColumnName("user_id")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(m => m.Role)
                .HasColumnName("role")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(m => m.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(m => m.JoinedAt)
                .HasColumnName("joined_at")
                .IsRequired();

            builder.Property(m => m.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(m => m.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(m => m.DeletedAt)
                .HasColumnName("deleted_at");

            // Unique constraint scoped to non-deleted rows
            builder.HasIndex(m => new { m.OrganizationId, m.UserId })
                .IsUnique()
                .HasDatabaseName("ix_memberships_org_user")
                .HasFilter("deleted_at IS NULL");

            builder.HasOne(m => m.Organization)
                .WithMany(o => o.Memberships)
                .HasForeignKey(m => m.OrganizationId)
                .HasConstraintName("fk_memberships_organization_id")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.User)
                .WithMany(u => u.Memberships)
                .HasForeignKey(m => m.UserId)
                .HasConstraintName("fk_memberships_user_id")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(m => m.DeletedAt == null);
        }
    }
}
