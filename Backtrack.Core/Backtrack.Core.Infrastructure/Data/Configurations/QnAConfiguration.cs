using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class QnAConfiguration : IEntityTypeConfiguration<QnA>
{
    public void Configure(EntityTypeBuilder<QnA> builder)
    {
        builder.ToTable("qna");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(q => q.AskerId)
            .HasColumnName("asker_id")
            .HasColumnType("text")
            .IsRequired();

        builder.HasOne(q => q.Asker)
            .WithMany()
            .HasForeignKey(q => q.AskerId)
            .HasConstraintName("fk_qna_asker_id_users_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(q => q.AskerId)
            .HasDatabaseName("ix_qna_asker_id");

        builder.Property(q => q.QuestionText)
            .HasColumnName("question_text")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(q => q.AnswererId)
            .HasColumnName("answerer_id")
            .HasColumnType("text");

        builder.HasOne(q => q.Answerer)
            .WithMany()
            .HasForeignKey(q => q.AnswererId)
            .HasConstraintName("fk_qna_answerer_id_users_id")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(q => q.AnswererId)
            .HasDatabaseName("ix_qna_answerer_id");

        builder.Property(q => q.AnswerText)
            .HasColumnName("answer_text")
            .HasMaxLength(2000);

        builder.Property(q => q.AnsweredAt)
            .HasColumnName("answered_at");

        builder.Property(q => q.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(q => q.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(q => q.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasIndex(q => q.CreatedAt)
            .HasDatabaseName("ix_qna_created_at");

        builder.HasQueryFilter(q => q.DeletedAt == null);
    }
}
