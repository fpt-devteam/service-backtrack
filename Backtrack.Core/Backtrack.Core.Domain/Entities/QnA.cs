using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Domain.Entities;

/// <summary>
/// Represents a question posted on a found-item post.
/// A question can have many answers from different users.
/// </summary>
public sealed class Question : Entity<Guid>
{
    public required Guid PostId { get; set; }
    public Post Post { get; set; } = default!;

    /// <summary>The Firebase UID of the user who asked the question.</summary>
    public required string AskerId { get; set; }

    /// <summary>Navigation property for the asker.</summary>
    public User Asker { get; set; } = default!;

    /// <summary>The text of the question.</summary>
    public required string QuestionText { get; set; }

    public ICollection<Answer> Answers { get; set; } = [];
}

/// <summary>
/// Represents a single answer to a question.
/// Multiple users can answer the same question.
/// </summary>
public sealed class Answer : Entity<Guid>
{
    public required Guid QuestionId { get; set; }
    public Question Question { get; set; } = default!;

    /// <summary>The Firebase UID of the user who answered the question.</summary>
    public required string AnswererId { get; set; }

    /// <summary>Navigation property for the answerer.</summary>
    public User Answerer { get; set; } = default!;

    public AnswerType Type { get; set; } = AnswerType.Text;

    /// <summary>Text content — populated when Type is Text.</summary>
    public string? AnswerText { get; set; }

    /// <summary>Image URLs — populated when Type is Image.</summary>
    public List<string>? ImageUrls { get; set; }
}
