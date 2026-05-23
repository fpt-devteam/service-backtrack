using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.GetQuestions;

/// <summary>
/// Handler for retrieving a paginated list of questions for a post.
/// </summary>
public sealed class GetQuestionsHandler(IQnARepository qnARepository)
    : IRequestHandler<GetQuestionsQuery, PagedResult<QuestionResult>>
{
    public async Task<PagedResult<QuestionResult>> Handle(
        GetQuestionsQuery query,
        CancellationToken cancellationToken)
    {
        var offset = (query.Page - 1) * query.PageSize;

        var (items, total) = await qnARepository.GetPagedByPostAsync(query.PostId, offset, query.PageSize, cancellationToken);

        var results = items.Select(q => q.ToQuestionResult()).ToList();

        return new PagedResult<QuestionResult>(total, results);
    }
}
