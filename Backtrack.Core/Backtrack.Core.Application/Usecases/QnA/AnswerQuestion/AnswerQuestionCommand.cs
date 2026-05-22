using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.AnswerQuestion;

/// <summary>
/// Command to add or update the answer on an existing question.
/// AnswererId and QnAId are injected by the controller.
/// </summary>
public sealed record AnswerQuestionCommand : IRequest<QnAResult>
{
    [JsonIgnore]
    public Guid QnAId { get; init; }

    [JsonIgnore]
    public string AnswererId { get; init; } = string.Empty;

    public required string AnswerText { get; init; }
}
