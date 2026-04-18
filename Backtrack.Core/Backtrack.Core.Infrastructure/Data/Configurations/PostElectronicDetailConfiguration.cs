using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class PostElectronicDetailConfiguration
    : IEntityTypeConfiguration<PostElectronicDetail>
{
    public void Configure(EntityTypeBuilder<PostElectronicDetail> builder)
    {
        builder.ToTable("post_electronic_details");
        builder.HasKey(d => d.PostId);

        builder.Property(d => d.PostId).HasColumnName("post_id");

        builder.Property(d => d.Brand).HasColumnName("brand").HasMaxLength(100);
        builder.Property(d => d.Model).HasColumnName("model").HasMaxLength(100);
        builder.Property(d => d.Color).HasColumnName("color").HasMaxLength(50);

        builder.HasIndex(d => new { d.Brand, d.Model })
            .HasDatabaseName("ix_post_electronic_details_brand_model");

        builder.Property(d => d.HasCase).HasColumnName("has_case");
        builder.Property(d => d.CaseDescription).HasColumnName("case_description").HasMaxLength(300);
        builder.Property(d => d.ScreenCondition).HasColumnName("screen_condition").HasMaxLength(50);
        builder.Property(d => d.LockScreenDescription).HasColumnName("lock_screen_description").HasMaxLength(500);
        builder.Property(d => d.DistinguishingFeatures).HasColumnName("distinguishing_features").HasMaxLength(500);
        builder.Property(d => d.AiDescription).HasColumnName("ai_description");
        builder.Property(d => d.AdditionalDetails).HasColumnName("additional_details").HasMaxLength(2000);
        builder.Property(d => d.ContentHash).HasColumnName("content_hash").HasMaxLength(64);

        builder.HasOne(d => d.Post)
            .WithOne(p => p.ElectronicDetail)
            .HasForeignKey<PostElectronicDetail>(d => d.PostId)
            .HasConstraintName("fk_post_electronic_details_post_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
