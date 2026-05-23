using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.GetQuestion;

/// <summary>
/// Handler for retrieving a single question by ID, including all its answers.
/// </summary>
public sealed class GetQuestionHandler(IQnARepository qnARepository) : IRequestHandler<GetQuestionQuery, QuestionResult>
{
    public async Task<QuestionResult> Handle(GetQuestionQuery query, CancellationToken cancellationToken)
    {
        var question = await qnARepository.GetByIdAsync(query.QnAId)
            ?? throw new NotFoundException(QnAErrors.NotFound);

        return question.ToQuestionResult();
    }
}
