using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasColumnName("id")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(255);

            builder.HasIndex(u => u.Email)
                .HasDatabaseName("ix_users_email");

            builder.Property(u => u.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(255);

            builder.Property(u => u.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(u => u.GlobalRole)
                .HasColumnName("global_role")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(u => u.DeletedAt)
                .HasColumnName("deleted_at");

            // Global query filter for soft delete
            builder.HasQueryFilter(u => u.DeletedAt == null);
        }
    }
}
