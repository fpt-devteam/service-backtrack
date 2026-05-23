using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class QnAConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(q => q.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.HasOne(q => q.Post)
            .WithMany(p => p.Questions)
            .HasForeignKey(q => q.PostId)
            .HasConstraintName("fk_questions_post_id_posts_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => q.PostId)
            .HasDatabaseName("ix_questions_post_id");

        builder.Property(q => q.AskerId)
            .HasColumnName("asker_id")
            .HasColumnType("text")
            .IsRequired();

        builder.HasOne(q => q.Asker)
            .WithMany()
            .HasForeignKey(q => q.AskerId)
            .HasConstraintName("fk_questions_asker_id_users_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(q => q.AskerId)
            .HasDatabaseName("ix_questions_asker_id");

        builder.Property(q => q.QuestionText)
            .HasColumnName("question_text")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(q => q.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(q => q.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(q => q.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasIndex(q => q.CreatedAt)
            .HasDatabaseName("ix_questions_created_at");

        builder.HasQueryFilter(q => q.DeletedAt == null);
    }
}
