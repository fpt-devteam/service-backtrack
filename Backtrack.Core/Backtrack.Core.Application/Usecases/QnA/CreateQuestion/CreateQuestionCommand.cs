using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.CreateQuestion;

/// <summary>
/// Command to create a new question. AskerId is injected from the auth header by the controller.
/// </summary>
public sealed record CreateQuestionCommand : IRequest<QnAResult>
{
    [JsonIgnore]
    public string AskerId { get; init; } = string.Empty;

    public required Guid PostId { get; init; }
    public required string QuestionText { get; init; }
}
