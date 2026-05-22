using Backtrack.Core.Application.Usecases.QnA.AnswerQuestion;
using Backtrack.Core.Application.Usecases.QnA.CreateQuestion;
using Backtrack.Core.Application.Usecases.QnA.GetQuestion;
using Backtrack.Core.Application.Usecases.QnA.GetQuestions;
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
    /// Creates a new question. The authenticated user becomes the asker.
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
    /// Adds or updates the answer on an existing question.
    /// The authenticated user becomes the answerer.
    /// </summary>
    [HttpPut("{id:guid}/answer")]
    public async Task<IActionResult> AnswerQuestionAsync(
        [FromRoute] Guid id,
        [FromBody] AnswerQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var answererId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        command = command with { QnAId = id, AnswererId = answererId };

        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    /// <summary>
    /// Retrieves a single question by its ID.
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
    /// Returns a paginated list of all questions ordered by creation date descending.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetQuestionsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQuestionsQuery { Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }
}
