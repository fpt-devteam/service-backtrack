using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class PostPersonalBelongingDetailConfiguration
    : IEntityTypeConfiguration<PostPersonalBelongingDetail>
{
    public void Configure(EntityTypeBuilder<PostPersonalBelongingDetail> builder)
    {
        builder.ToTable("post_personal_belonging_details");
        builder.HasKey(d => d.PostId);

        builder.Property(d => d.PostId).HasColumnName("post_id");

        builder.Property(d => d.Color).HasColumnName("color").HasMaxLength(100);
        builder.Property(d => d.Brand).HasColumnName("brand").HasMaxLength(100);
        builder.Property(d => d.Material).HasColumnName("material").HasMaxLength(100);
        builder.Property(d => d.Size).HasColumnName("size").HasMaxLength(50);
        builder.Property(d => d.Condition).HasColumnName("condition").HasMaxLength(100);
        builder.Property(d => d.DistinctiveMarks).HasColumnName("distinctive_marks").HasMaxLength(500);
        builder.Property(d => d.AiDescription).HasColumnName("ai_description");
        builder.Property(d => d.AdditionalDetails).HasColumnName("additional_details").HasMaxLength(2000);
        builder.Property(d => d.ContentHash).HasColumnName("content_hash").HasMaxLength(64);

        builder.HasOne(d => d.Post)
            .WithOne(p => p.PersonalBelongingDetail)
            .HasForeignKey<PostPersonalBelongingDetail>(d => d.PostId)
            .HasConstraintName("fk_post_personal_belonging_details_post_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(d => d.Post.DeletedAt == null);
    }
}
