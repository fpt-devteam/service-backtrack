using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Usecases.QnA;

/// <summary>
/// Extension methods to map Question/Answer domain entities to result DTOs.
/// </summary>
public static class QnAMapper
{
    public static AnswerResult ToAnswerResult(this Answer answer)
    {
        return new AnswerResult
        {
            Id         = answer.Id,
            QuestionId = answer.QuestionId,
            AnswererId = answer.AnswererId,
            AnswerText = answer.AnswerText,
            CreatedAt  = answer.CreatedAt,
            UpdatedAt  = answer.UpdatedAt,
        };
    }

    public static QuestionResult ToQuestionResult(this Question question)
    {
        return new QuestionResult
        {
            Id           = question.Id,
            PostId       = question.PostId,
            AskerId      = question.AskerId,
            QuestionText = question.QuestionText,
            CreatedAt    = question.CreatedAt,
            UpdatedAt    = question.UpdatedAt,
        };
    }

    public static QuestionWithAnswersResult ToQuestionWithAnswersResult(this Question question)
    {
        return new QuestionWithAnswersResult
        {
            Id           = question.Id,
            PostId       = question.PostId,
            AskerId      = question.AskerId,
            QuestionText = question.QuestionText,
            Answers      = question.Answers.Select(a => a.ToAnswerResult()).ToList(),
            CreatedAt    = question.CreatedAt,
            UpdatedAt    = question.UpdatedAt,
        };
    }
}
