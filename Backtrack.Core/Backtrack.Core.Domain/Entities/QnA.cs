namespace Backtrack.Core.Domain.Entities;

/// <summary>
/// Represents a question and answer entry in the BackTrack platform.
/// The answer fields are null until a user answers the question.
/// </summary>
public sealed class QnA : Entity<Guid>
{
    public required Guid PostId { get; set; }
    public Post Post { get; set; } = default!;

    /// <summary>The Firebase UID of the user who asked the question.</summary>
    public required string AskerId { get; set; }

    /// <summary>Navigation property for the asker.</summary>
    public User Asker { get; set; } = default!;

    /// <summary>The text of the question.</summary>
    public required string QuestionText { get; set; }

    /// <summary>The Firebase UID of the user who answered the question. Null until answered.</summary>
    public string? AnswererId { get; set; }

    /// <summary>Navigation property for the answerer.</summary>
    public User? Answerer { get; set; }

    /// <summary>The text of the answer. Null until answered.</summary>
    public string? AnswerText { get; set; }

    /// <summary>Timestamp when the answer was provided.</summary>
    public DateTimeOffset? AnsweredAt { get; set; }
}
