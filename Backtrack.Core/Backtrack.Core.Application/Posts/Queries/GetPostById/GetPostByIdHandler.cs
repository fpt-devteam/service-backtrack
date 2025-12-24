using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Common.Exceptions.Errors;
using Backtrack.Core.Application.Posts.Common;
using MediatR;

namespace Backtrack.Core.Application.Posts.Queries.GetPostById;

public sealed class GetPostByIdHandler : IRequestHandler<GetPostByIdQuery, PostResult>
{
    private readonly IPostRepository _postRepository;

    public GetPostByIdHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PostResult> Handle(GetPostByIdQuery query, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(query.PostId);

        if (post == null)
        {
            throw new NotFoundException(PostErrors.NotFound);
        }

        return new PostResult
        {
            Id = post.Id,
            PostType = post.PostType.ToString(),
            ItemName = post.ItemName,
            Description = post.Description,
            ImageUrls = post.ImageUrls,
            Location = post.Location != null
                ? new LocationResult
                {
                    Latitude = post.Location.Latitude,
                    Longitude = post.Location.Longitude
                }
                : null,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        };
    }
}
