using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostImages;
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
            Author = post.Author?.ToAuthorResult(),
            Organization = post.Organization?.ToOrganizationOnPost(),
            PostType = post.PostType.ToString(),
            ItemName = post.ItemName,
            Description = post.Description,
            Images = post.Images.Select(i => i.ToPostImageResult()).ToList(),
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        }).ToList();
    }
}
