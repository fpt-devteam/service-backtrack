using Microsoft.AspNetCore.Mvc;
using MediatR;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.Application.Usecases.Posts.Commands.CreatePost;
using Backtrack.Core.Application.Usecases.Posts.Queries.GetPosts;
using Backtrack.Core.Application.Usecases.Posts.Queries.SearchPostsBySemantic;
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.Application.Usecases.Posts.Queries.GetPostById;
using Backtrack.Core.Application.Usecases.Posts.Queries.GetSimilarPosts;
using Backtrack.Core.Application.Usecases.Posts.Commands.DeletePost;
using Backtrack.Core.Application.Usecases.Posts;

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
    public async Task<IActionResult> CreatePostAsync([FromBody] CreatePostCommand command, CancellationToken cancellationToken)
    {
        var authorId = Request.Headers[HeaderNames.AuthId].ToString();
        command = command with { AuthorId = authorId };

        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPostsAsync([FromQuery] GetPostsQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);

        var response = PagedResponse<PostResult>.Create(
            items: result.Items,
            page: query.Page,
            pageSize: query.PageSize,
            totalCount: result.Total
        );

        return this.ApiOk(response);
    }

    [HttpGet("{postId:guid}")]
    public async Task<IActionResult> GetPostByIdAsync(
        [FromRoute] Guid postId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPostByIdQuery { PostId = postId };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("search/semantic")]
    public async Task<IActionResult> SearchPostsBySemanticAsync([FromQuery] SearchPostsBySemanticQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);

        var response = PagedResponse<PostSemanticSearchResult>.Create(
            items: result.Items,
            page: query.Page,
            pageSize: query.PageSize,
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
        var query = new GetSimilarPostsQuery
        {
            PostId = postId,
            Limit = limit
        };

        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpDelete("{postId:guid}")]
    public async Task<IActionResult> DeletePostAsync(
        [FromRoute] Guid postId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeletePostCommand { PostId = postId };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
