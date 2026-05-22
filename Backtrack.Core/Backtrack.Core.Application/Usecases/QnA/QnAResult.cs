namespace Backtrack.Core.Application.Usecases.QnA;

/// <summary>
/// DTO returned from QnA queries and commands.
/// </summary>
public sealed record QnAResult
{
    public Guid Id { get; init; }
    public Guid PostId { get; init; }

    public string AskerId { get; init; } = default!;
    public string QuestionText { get; init; } = default!;

    public string? AnswererId { get; init; }
    public string? AnswerText { get; init; }
    public DateTimeOffset? AnsweredAt { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
