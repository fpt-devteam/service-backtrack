using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QnA.CreateQuestions;

public sealed record CreateQuestionsCommand : IRequest<IReadOnlyList<QuestionResult>>
{
    [JsonIgnore]
    public string AskerId { get; init; } = string.Empty;

    public required Guid PostId { get; init; }
    public required IReadOnlyList<string> QuestionTexts { get; init; }
}
