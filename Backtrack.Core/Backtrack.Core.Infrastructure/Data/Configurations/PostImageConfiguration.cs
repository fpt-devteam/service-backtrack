using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class PostImageConfiguration : IEntityTypeConfiguration<PostImage>
{
    public void Configure(EntityTypeBuilder<PostImage> builder)
    {
        builder.ToTable("post_images");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(i => i.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(i => i.Url)
            .HasColumnName("url")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(i => i.Base64Data)
            .HasColumnName("base64_data")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(i => i.MimeType)
            .HasColumnName("mime_type")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(255);

        builder.Property(i => i.FileSizeBytes)
            .HasColumnName("file_size_bytes");

        builder.Property(i => i.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(i => i.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasOne(i => i.Post)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.PostId)
            .HasConstraintName("fk_post_images_post_id_posts_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.PostId)
            .HasDatabaseName("ix_post_images_post_id");

        builder.HasQueryFilter(i => i.DeletedAt == null);
    }
}
