using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.GetQuestionsWithAnswers;

public sealed class GetQuestionsWithAnswersHandler(IQnARepository qnARepository)
    : IRequestHandler<GetQuestionsWithAnswersQuery, IReadOnlyList<QuestionWithAnswersResult>>
{
    public async Task<IReadOnlyList<QuestionWithAnswersResult>> Handle(
        GetQuestionsWithAnswersQuery query,
        CancellationToken cancellationToken)
    {
        var questions = await qnARepository.GetWithAnswersByPostAsync(query.PostId, query.AnswererId, cancellationToken);

        return [..questions.Select(q => q.ToQuestionWithAnswersResult())];
    }
}
