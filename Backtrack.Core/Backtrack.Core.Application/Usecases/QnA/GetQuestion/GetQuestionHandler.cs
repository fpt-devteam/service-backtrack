using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.GetQuestion;

/// <summary>
/// Handler for retrieving a single question by ID.
/// </summary>
public sealed class GetQuestionHandler(IQnARepository qnARepository) : IRequestHandler<GetQuestionQuery, QnAResult>
{
    public async Task<QnAResult> Handle(GetQuestionQuery query, CancellationToken cancellationToken)
    {
        var qna = await qnARepository.GetByIdAsync(query.QnAId)
            ?? throw new NotFoundException(QnAErrors.NotFound);

        return qna.ToQnAResult();
    }
}
