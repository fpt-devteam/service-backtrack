using Microsoft.AspNetCore.Mvc;
using MediatR;
using Backtrack.Core.WebApi.Mappings;
using Backtrack.Core.WebApi.Contracts.Posts.Requests;
using Backtrack.Core.WebApi.Contracts.Posts.Responses;
using Backtrack.Core.WebApi.Contracts.Common;
using Backtrack.Core.WebApi.Constants;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("posts")]
public class PostController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PostController> _logger;

    public PostController(IMediator mediator, ILogger<PostController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePostAsync([FromBody] CreatePostRequest request, CancellationToken cancellationToken)
    {
        var authorId = Request.Headers[HeaderNames.AuthId].ToString();

        var command = request.ToCommand(authorId);
        var result = await _mediator.Send(command, cancellationToken);
        var response = result.ToResponse();

        return this.ApiCreated(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetPostsAsync([FromQuery] GetPostsRequest request, CancellationToken cancellationToken = default)
    {
        var query = request.ToQuery();
        var result = await _mediator.Send(query, cancellationToken);

        var response = PagedResponse<PostResponse>.Create(
            items: result.Items.Select(item => item.ToResponse()),
            page: request.Page,
            pageSize: request.PageSize,
            totalCount: result.Total
        );

        return this.ApiOk(response);
    }

    [HttpGet("{postId:guid}")]
    public async Task<IActionResult> GetPostByIdAsync(
        [FromRoute] Guid postId,
        CancellationToken cancellationToken = default)
    {
        var query = PostMappings.ToQuery(postId);
        var result = await _mediator.Send(query, cancellationToken);
        var response = result.ToResponse();

        return this.ApiOk(response);
    }

    [HttpGet("search/semantic")]
    public async Task<IActionResult> SearchPostsBySemanticAsync([FromQuery] SearchPostsBySemanticRequest request, CancellationToken cancellationToken = default)
    {
        var query = request.ToQuery();
        var result = await _mediator.Send(query, cancellationToken);

        var response = PagedResponse<PostSemanticSearchResponse>.Create(
            items: result.Items.Select(item => item.ToResponse()),
            page: request.Page,
            pageSize: request.PageSize,
            totalCount: result.Total
        );

        return this.ApiOk(response);
    }

    /// <summary>
    /// Get posts similar to a specific post using vector similarity and geographic proximity
    /// </summary>
    /// <param name="postId">ID of the post to find similar items for</param>
    /// <param name="limit">Maximum number of similar posts to return (default: 20, max: 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of similar posts sorted by similarity score</returns>
    [HttpGet("{postId:guid}/similar")]
    public async Task<IActionResult> GetSimilarPostsAsync(
        [FromRoute] Guid postId,
        [FromQuery] int limit = 5,
        CancellationToken cancellationToken = default)
    {
        var request = new GetSimilarPostsRequest
        {
            PostId = postId,
            Limit = limit
        };

        var query = request.ToQuery();
        var result = await _mediator.Send(query, cancellationToken);
        var response = result.ToResponse();

        return this.ApiOk(response);
    }

    [HttpDelete("{postId:guid}")]
    public async Task<IActionResult> DeletePostAsync(
        [FromRoute] Guid postId,
        CancellationToken cancellationToken = default)
    {
        var command = PostMappings.ToCommand(postId);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
