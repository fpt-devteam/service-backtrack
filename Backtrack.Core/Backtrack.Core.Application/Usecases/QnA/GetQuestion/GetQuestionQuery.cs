using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.GetQuestion;

/// <summary>
/// Query to retrieve a single question by its identifier.
/// </summary>
public sealed record GetQuestionQuery : IRequest<QnAResult>
{
    public required Guid QnAId { get; init; }
}
