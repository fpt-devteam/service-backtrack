using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.GetQuestions;

/// <summary>
/// Query to retrieve a paginated list of all questions.
/// </summary>
public sealed record GetQuestionsQuery : IRequest<PagedResult<QnAResult>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
