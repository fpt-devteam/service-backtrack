using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.QnA;

/// <summary>
/// DTO for a single answer on a question.
/// </summary>
public sealed record AnswerResult
{
    public Guid Id { get; init; }
    public Guid QuestionId { get; init; }
    public string AnswererId { get; init; } = default!;
    public AnswerType Type { get; init; }
    public string? AnswerText { get; init; }
    public IReadOnlyList<string>? ImageUrls { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>
/// DTO returned from QnA queries and commands.
/// </summary>
public sealed record QuestionResult
{
    public Guid Id { get; init; }
    public Guid PostId { get; init; }
    public string AskerId { get; init; } = default!;
    public string QuestionText { get; init; } = default!;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>
/// DTO pairing a question with all its answers — used for the full Q&A view of a post.
/// </summary>
public sealed record QuestionWithAnswersResult
{
    public Guid Id { get; init; }
    public Guid PostId { get; init; }
    public string AskerId { get; init; } = default!;
    public string QuestionText { get; init; } = default!;
    public IReadOnlyList<AnswerResult> Answers { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
