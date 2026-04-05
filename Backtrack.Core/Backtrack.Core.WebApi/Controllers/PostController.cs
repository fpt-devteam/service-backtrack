using Microsoft.AspNetCore.Mvc;
using MediatR;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using Backtrack.Core.Application.Usecases.Posts.CreatePost;
using Backtrack.Core.Application.Usecases.Posts.GetPostById;
using Backtrack.Core.Application.Usecases.PostMatchings.GetSimilarPosts;
using Backtrack.Core.Application.Usecases.Posts.DeletePost;
using Backtrack.Core.Application.Usecases.Posts.GetPostsByAuthorId;
using Backtrack.Core.Application.Usecases.Posts.UpdatePost;
using Backtrack.Core.Application.Usecases.PostMatchings.GetPostMatchingStatus;
using Backtrack.Core.Application.Usecases.PostExplorations.SearchPosts;
using Backtrack.Core.Application.Usecases.PostExplorations.GetFeed;
using Backtrack.Core.WebApi.Common;

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
        var authorId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var orgIdHeader = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.OrgId);

        _logger.LogInformation("Creating post for author {AuthorId} with organization {OrgId}", authorId, orgIdHeader);

        Guid? organizationId = null;
        if (Guid.TryParse(orgIdHeader, out var parsedOrgId))
        {
            organizationId = parsedOrgId;
        }
        _logger.LogInformation("Parsed organization ID: {OrganizationId}", organizationId);


        command = command with { AuthorId = authorId, OrganizationId = organizationId };

        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiCreated(result);
    }

    [HttpPut("{postId:guid}")]
    public async Task<IActionResult> UpdatePostAsync(
        [FromRoute] Guid postId,
        [FromBody] UpdatePostCommand command,
        CancellationToken cancellationToken)
    {
        var authorId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var orgIdHeader = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.OrgId);

        Guid? organizationId = null;
        if (Guid.TryParse(orgIdHeader, out var parsedOrgId))
        {
            organizationId = parsedOrgId;
        }

        command = command with { PostId = postId, UserId = authorId, OrganizationId = organizationId };

        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyPostsAsync(CancellationToken cancellationToken = default)
    {
        var authorId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var query = new GetPostsByAuthorIdQuery(authorId);
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchPostsAsync([FromBody] SearchPostsCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpPost("feed")]
    public async Task<IActionResult> GetFeedAsync([FromBody] GetFeedQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
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

    [HttpGet("{postId:guid}/matching-status")]
    public async Task<IActionResult> GetPostMatchingStatusAsync(
        [FromRoute] Guid postId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPostMatchingStatusQuery { PostId = postId };
        var result = await _mediator.Send(query, cancellationToken);
        return this.ApiOk(result);
    }

    [HttpDelete("{postId:guid}")]
    public async Task<IActionResult> DeletePostAsync(
        [FromRoute] Guid postId,
        CancellationToken cancellationToken = default)
    {
        var authorId = HttpContextUtil.GetHeaderValue(HttpContext, HeaderNames.AuthId);
        var command = new DeletePostCommand { PostId = postId, UserId = authorId };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
