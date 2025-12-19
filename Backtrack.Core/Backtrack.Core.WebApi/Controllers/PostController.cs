using Microsoft.AspNetCore.Mvc;
using MediatR;
using Backtrack.Core.WebApi.Mappings;
using Backtrack.Core.WebApi.Contracts.Posts.Requests;
using Backtrack.Core.WebApi.Contracts.Posts.Responses;
using Backtrack.Core.WebApi.Contracts.Common;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("posts")]
public class PostController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePostAsync(CreatePostRequest request, CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
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
}
