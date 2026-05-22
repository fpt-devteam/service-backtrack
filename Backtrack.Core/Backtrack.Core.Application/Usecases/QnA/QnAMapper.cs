namespace Backtrack.Core.Application.Usecases.QnA;

/// <summary>
/// Extension methods to map QnA domain entities to QnAResult DTOs.
/// </summary>
public static class QnAMapper
{
    public static QnAResult ToQnAResult(this Backtrack.Core.Domain.Entities.QnA qna)
    {
        return new QnAResult
        {
            Id           = qna.Id,
            AskerId      = qna.AskerId,
            QuestionText = qna.QuestionText,
            AnswererId   = qna.AnswererId,
            AnswerText   = qna.AnswerText,
            AnsweredAt   = qna.AnsweredAt,
            CreatedAt    = qna.CreatedAt,
            UpdatedAt    = qna.UpdatedAt,
        };
    }
}
