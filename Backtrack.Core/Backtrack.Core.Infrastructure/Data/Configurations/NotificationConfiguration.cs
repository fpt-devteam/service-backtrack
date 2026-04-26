using System.Text.Json;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(n => n.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
        builder.Property(n => n.Body).HasColumnName("body").HasMaxLength(2000).IsRequired();

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(n => n.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        var dataConverter = new ValueConverter<NotificationData?, string?>(
            v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => v == null ? null : JsonSerializer.Deserialize<NotificationData>(v, (JsonSerializerOptions?)null));

        builder.Property(n => n.Data)
            .HasColumnName("data")
            .HasColumnType("jsonb")
            .HasConversion(dataConverter);
        builder.Property(n => n.SourceName).HasColumnName("source_name").HasMaxLength(255).IsRequired();
        builder.Property(n => n.SourceEventId).HasColumnName("source_event_id").HasMaxLength(255).IsRequired();
        builder.Property(n => n.SentAt).HasColumnName("sent_at");
        builder.Property(n => n.CreatedAt).HasColumnName("created_at");
        builder.Property(n => n.UpdatedAt).HasColumnName("updated_at");
        builder.Property(n => n.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .HasConstraintName("fk_notifications_user_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(n => n.DeletedAt == null);

        builder.HasIndex(n => new { n.UserId, n.CreatedAt })
            .HasDatabaseName("ix_notifications_user_id_created_at");

        builder.HasIndex(n => new { n.UserId, n.Status })
            .HasDatabaseName("ix_notifications_user_id_status");

        builder.HasIndex(n => new { n.SourceName, n.SourceEventId })
            .IsUnique()
            .HasDatabaseName("ix_notifications_source_name_event_id");
    }
}
