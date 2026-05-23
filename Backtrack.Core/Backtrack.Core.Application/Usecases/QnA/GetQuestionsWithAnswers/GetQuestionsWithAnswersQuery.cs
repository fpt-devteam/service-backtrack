using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.GetQuestionsWithAnswers;

public sealed record GetQuestionsWithAnswersQuery : IRequest<IReadOnlyList<QuestionWithAnswersResult>>
{
    public required Guid PostId { get; init; }

    /// <summary>
    /// When provided, only answers from this user are included.
    /// Null returns all answers from all users.
    /// </summary>
    public string? AnswererId { get; init; }
}
