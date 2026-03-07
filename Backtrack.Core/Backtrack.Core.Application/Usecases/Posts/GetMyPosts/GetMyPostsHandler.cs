using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetMyPosts;

public sealed class GetMyPostsHandler : IRequestHandler<GetMyPostsQuery, List<PostResult>>
{
    private readonly IPostRepository _postRepository;

    public GetMyPostsHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<List<PostResult>> Handle(GetMyPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetByAuthorIdAsync(request.UserId, cancellationToken);

        return posts.Select(post => new PostResult
        {
            Id = post.Id,
            Author = new AuthorResult
            {
                Id = post.Author.Id,
                DisplayName = post.Author.DisplayName,
                AvatarUrl = post.Author.AvatarUrl
            },
            PostType = post.PostType.ToString(),
            ItemName = post.ItemName,
            Description = post.Description,
            ImageUrls = post.ImageUrls,
            Location = new LocationResult
            {
                Latitude = post.Location.Latitude,
                Longitude = post.Location.Longitude
            },
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        }).ToList();
    }
}
