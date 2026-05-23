using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.CreateAnswer;

/// <summary>
/// Command to add an answer to an existing question.
/// QuestionId and AnswererId are injected by the controller.
/// </summary>
public sealed record CreateAnswerCommand : IRequest<AnswerResult>
{
    [JsonIgnore]
    public Guid QuestionId { get; init; }

    [JsonIgnore]
    public required string AnswererId { get; init; }

    public required string AnswerText { get; init; }
}
