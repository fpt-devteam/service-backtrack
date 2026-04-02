using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetPostsByAuthorId;

public sealed class GetPostsByAuthorIdHandler : IRequestHandler<GetPostsByAuthorIdQuery, List<PostResult>>
{
    private readonly IPostRepository _postRepository;

    public GetPostsByAuthorIdHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<List<PostResult>> Handle(GetPostsByAuthorIdQuery request, CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetByAuthorIdAsync(request.AuthorId, cancellationToken);

        return posts.Select(post => new PostResult
        {
            Id = post.Id,
            Author = post.Author?.ToPostAuthorResult(),
            Organization = post.Organization?.ToOrganizationOnPost(),
            PostType = post.PostType,
            Item = post.Item,
            ImageUrls = post.ImageUrls,
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        }).ToList();
    }
}
