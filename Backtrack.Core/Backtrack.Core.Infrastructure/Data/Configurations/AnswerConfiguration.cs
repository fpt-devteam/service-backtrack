using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("answers");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(a => a.QuestionId)
            .HasColumnName("question_id")
            .IsRequired();

        builder.HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .HasConstraintName("fk_answers_question_id_questions_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.QuestionId)
            .HasDatabaseName("ix_answers_question_id");

        builder.Property(a => a.AnswererId)
            .HasColumnName("answerer_id")
            .HasColumnType("text")
            .IsRequired();

        builder.HasOne(a => a.Answerer)
            .WithMany()
            .HasForeignKey(a => a.AnswererId)
            .HasConstraintName("fk_answers_answerer_id_users_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.AnswererId)
            .HasDatabaseName("ix_answers_answerer_id");

        builder.Property(a => a.AnswerText)
            .HasColumnName("answer_text")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(a => a.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasQueryFilter(a => a.DeletedAt == null);
    }
}
