using Microsoft.AspNetCore.Mvc;
using Backtrack.Core.Application.Posts.Commands.CreatePost;
using Backtrack.Core.Application.Posts.Queries.GetPosts;
using Backtrack.Core.Contract.Posts.Requests;
using Backtrack.Core.Contract.Posts.Responses;
using Backtrack.Core.WebApi.Extensions;
using MediatR;
using Backtrack.Core.Application.Posts.Common;
using Backtrack.Core.Application.Common;
using Backtrack.Core.Contract.Common;

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
        var command = new CreatePostCommand
        {
            PostType = request.PostType,
            ItemName = request.ItemName,
            Description = request.Description,
            Material = request.Material,
            Brands = request.Brands,
            Colors = request.Colors,
            ImageUrls = request.ImageUrls,
            Location = request.Location != null
                ? new LocationDto
                {
                    Latitude = request.Location.Latitude,
                    Longitude = request.Location.Longitude
                }
                : null,
            EventTime = request.EventTime
        };

        PostResult result = await _mediator.Send(command, cancellationToken);

        PostResponse response = new PostResponse
        {
            Id = result.Id,
            PostType = result.PostType,
            ItemName = result.ItemName,
            Description = result.Description,
            Material = result.Material,
            Brands = result.Brands,
            Colors = result.Colors,
            ImageUrls = result.ImageUrls,
            Location = result.Location != null
                ? new LocationResponse
                {
                    Latitude = result.Location.Latitude,
                    Longitude = result.Location.Longitude
                }
                : null,
            EventTime = result.EventTime,
            CreatedAt = result.CreatedAt,
            DistanceInKm = result.DistanceInKm
        };

        return this.ApiCreated(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetPostsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? postType = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] double? latitude = null,
        [FromQuery] double? longitude = null,
        [FromQuery] double? radiusInKm = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPostsQuery(
            PagedQuery.FromPage(page, pageSize),
            postType,
            searchTerm,
            latitude,
            longitude,
            radiusInKm
        );

        PagedResult<PostResult> result = await _mediator.Send(query, cancellationToken);

        var response = PagedResponse<PostResponse>.Create(
            items: result.Items.Select(item => new PostResponse
            {
                Id = item.Id,
                PostType = item.PostType,
                ItemName = item.ItemName,
                Description = item.Description,
                Material = item.Material,
                Brands = item.Brands,
                Colors = item.Colors,
                ImageUrls = item.ImageUrls,
                Location = item.Location != null
                    ? new LocationResponse
                    {
                        Latitude = item.Location.Latitude,
                        Longitude = item.Location.Longitude
                    }
                    : null,
                EventTime = item.EventTime,
                CreatedAt = item.CreatedAt,
                DistanceInKm = item.DistanceInKm
            }),
            page: page,
            pageSize: pageSize,
            totalCount: result.Total
        );

        return this.ApiOk(response);
    }
}
