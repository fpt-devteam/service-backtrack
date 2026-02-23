using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class JoinInvitationConfiguration : IEntityTypeConfiguration<JoinInvitation>
{
    public void Configure(EntityTypeBuilder<JoinInvitation> builder)
    {
        builder.ToTable("join_invitations");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(j => j.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(j => j.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(j => j.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(j => j.HashCode)
            .HasColumnName("hash_code")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(j => j.ExpiredTime)
            .HasColumnName("expired_time")
            .IsRequired();

        builder.Property(j => j.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(j => j.InvitedBy)
            .HasColumnName("invited_by")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(j => j.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(j => j.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(j => j.DeletedAt)
            .HasColumnName("deleted_at");

        // Unique index on hash_code
        builder.HasIndex(j => j.HashCode)
            .IsUnique()
            .HasDatabaseName("ix_join_invitations_hash_code");

        // Index on email for lookups
        builder.HasIndex(j => j.Email)
            .HasDatabaseName("ix_join_invitations_email");

        // FK to organizations
        builder.HasOne(j => j.Organization)
            .WithMany()
            .HasForeignKey(j => j.OrganizationId)
            .HasConstraintName("fk_join_invitations_organization_id")
            .OnDelete(DeleteBehavior.Cascade);

        // FK to users (InvitedBy)
        builder.HasOne(j => j.InviterUser)
            .WithMany()
            .HasForeignKey(j => j.InvitedBy)
            .HasConstraintName("fk_join_invitations_invited_by")
            .OnDelete(DeleteBehavior.NoAction);

        // Global query filter for soft delete
        builder.HasQueryFilter(j => j.DeletedAt == null);
    }
}
