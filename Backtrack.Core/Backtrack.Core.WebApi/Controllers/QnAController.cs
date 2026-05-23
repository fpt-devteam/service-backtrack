using Backtrack.Core.Application.Usecases.QnA.CreateAnswer;
using Backtrack.Core.Application.Usecases.QnA.CreateQuestion;
using Backtrack.Core.Application.Usecases.QnA.CreateQuestions;
using Backtrack.Core.Application.Usecases.QnA.GetQuestion;
using Backtrack.Core.Application.Usecases.QnA.GetQuestions;
using Backtrack.Core.Application.Usecases.QnA.GetQuestionsWithAnswers;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

/// <summary>
/// Manages questions and answers (QnA) in the BackTrack platform.
/// </summary>
[ApiController]
[Route("qna")]
public class QnAController : ControllerBase
{
    private readonly IMediator _mediator;

    public QnAController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new question on a found-item post. The authenticated user becomes the asker.
    /// Only posts of type Found allow questions.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateQuestionAsync(
        [FromBody] CreateQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var askerId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { AskerId = askerId };

        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    /// <summary>
    /// Creates multiple questions at once on a found-item post (used during post setup).
    /// </summary>
    [HttpPost("batch")]
    public async Task<IActionResult> CreateQuestionsAsync(
        [FromBody] CreateQuestionsCommand command,
        CancellationToken cancellationToken)
    {
        var askerId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { AskerId = askerId };

        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    /// <summary>
    /// Adds an answer to an existing question. The authenticated user becomes the answerer.
    /// Multiple users can answer the same question.
    /// </summary>
    [HttpPost("{id:guid}/answers")]
    public async Task<IActionResult> CreateAnswerAsync(
        [FromRoute] Guid id,
        [FromBody] CreateAnswerCommand command,
        CancellationToken cancellationToken)
    {
        var answererId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { QuestionId = id, AnswererId = answererId };

        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    /// <summary>
    /// Retrieves a single question by its ID, including all answers.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetQuestionAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetQuestionQuery { QnAId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Returns all questions for a post together with their answers.
    /// </summary>
    [HttpGet("with-answers")]
    public async Task<IActionResult> GetQuestionsWithAnswersAsync(
        [FromQuery] Guid postId,
        [FromQuery] string? answererId,
        CancellationToken cancellationToken)
    {
        var query = new GetQuestionsWithAnswersQuery { PostId = postId, AnswererId = answererId };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Returns a paginated list of questions for a post, ordered by creation date descending.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetQuestionsAsync(
        [FromQuery] Guid postId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionsQuery { PostId = postId, Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }
}
