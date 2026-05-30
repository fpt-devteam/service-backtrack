using System.Text.Json.Serialization;
using Backtrack.Core.Domain.Constants;
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
    public string AnswererId { get; init; } = string.Empty;

    public AnswerType Type { get; init; } = AnswerType.Text;

    /// <summary>Required when Type is Text.</summary>
    public string? AnswerText { get; init; }

    /// <summary>Required when Type is Image.</summary>
    public IReadOnlyList<string>? ImageUrls { get; init; }
}
