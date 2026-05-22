using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.GetQuestions;

/// <summary>
/// Handler for retrieving a paginated list of questions.
/// </summary>
public sealed class GetQuestionsHandler(IQnARepository qnARepository)
    : IRequestHandler<GetQuestionsQuery, PagedResult<QnAResult>>
{
    public async Task<PagedResult<QnAResult>> Handle(
        GetQuestionsQuery query,
        CancellationToken cancellationToken)
    {
        var offset = (query.Page - 1) * query.PageSize;

        var (items, total) = await qnARepository.GetPagedByPostAsync(query.PostId, offset, query.PageSize, cancellationToken);

        var results = items.Select(qna => qna.ToQnAResult()).ToList();

        return new PagedResult<QnAResult>(total, results);
    }
}
