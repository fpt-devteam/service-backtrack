using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class PostCardDetailConfiguration : IEntityTypeConfiguration<PostCardDetail>
{
    public void Configure(EntityTypeBuilder<PostCardDetail> builder)
    {
        builder.ToTable("post_card_details");
        builder.HasKey(d => d.PostId);

        builder.Property(d => d.PostId).HasColumnName("post_id");

        // Sensitive (hash only)
        builder.Property(d => d.CardNumberHash).HasColumnName("card_number_hash").HasMaxLength(128);
        builder.Property(d => d.CardNumberMasked).HasColumnName("card_number_masked").HasMaxLength(20);

        builder.HasIndex(d => d.CardNumberHash)
            .HasDatabaseName("ix_post_card_details_card_number_hash")
            .HasFilter("card_number_hash IS NOT NULL");

        // Plain for fuzzy match
        builder.Property(d => d.HolderName).HasColumnName("holder_name").HasMaxLength(200);
        builder.Property(d => d.HolderNameNormalized).HasColumnName("holder_name_normalized").HasMaxLength(200);
        builder.Property(d => d.DateOfBirth).HasColumnName("date_of_birth");

        builder.HasIndex(d => d.DateOfBirth)
            .HasDatabaseName("ix_post_card_details_dob")
            .HasFilter("date_of_birth IS NOT NULL");

        builder.Property(d => d.ItemName).HasColumnName("item_name").HasMaxLength(300);

        // Other card metadata
        builder.Property(d => d.IssueDate).HasColumnName("issue_date");
        builder.Property(d => d.ExpiryDate).HasColumnName("expiry_date");
        builder.Property(d => d.IssuingAuthority).HasColumnName("issuing_authority").HasMaxLength(300);
        builder.Property(d => d.OcrText).HasColumnName("ocr_text");
        builder.Property(d => d.AdditionalDetails).HasColumnName("additional_details").HasMaxLength(2000);
        builder.Property(d => d.AiDescription).HasColumnName("ai_description");
        builder.Property(d => d.ContentHash).HasColumnName("content_hash").HasMaxLength(64);

        // Trigram index for holder_name_normalized must be added manually in migration:
        // CREATE EXTENSION IF NOT EXISTS pg_trgm;
        // CREATE INDEX ix_post_card_details_holder_name_trgm
        //   ON post_card_details USING gin (holder_name_normalized gin_trgm_ops);

        builder.HasOne(d => d.Post)
            .WithOne(p => p.CardDetail)
            .HasForeignKey<PostCardDetail>(d => d.PostId)
            .HasConstraintName("fk_post_card_details_post_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(d => d.Post.DeletedAt == null);
    }
}
