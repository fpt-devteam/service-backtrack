using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class PostOtherDetailConfiguration : IEntityTypeConfiguration<PostOtherDetail>
{
    public void Configure(EntityTypeBuilder<PostOtherDetail> builder)
    {
        builder.ToTable("post_other_details");
        builder.HasKey(d => d.PostId);

        builder.Property(d => d.PostId).HasColumnName("post_id");

        builder.Property(d => d.ItemName)
            .HasColumnName("item_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.PrimaryColor).HasColumnName("primary_color").HasMaxLength(100);
        builder.Property(d => d.AdditionalDetails).HasColumnName("additional_details").HasMaxLength(2000);
        builder.Property(d => d.AiDescription).HasColumnName("ai_description");
        builder.Property(d => d.ContentHash).HasColumnName("content_hash").HasMaxLength(64);

        builder.HasOne(d => d.Post)
            .WithOne(p => p.OtherDetail)
            .HasForeignKey<PostOtherDetail>(d => d.PostId)
            .HasConstraintName("fk_post_other_details_post_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(d => d.Post.DeletedAt == null);
    }
}
