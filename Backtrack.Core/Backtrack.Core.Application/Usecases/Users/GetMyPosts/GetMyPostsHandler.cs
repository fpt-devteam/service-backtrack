using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Users.GetMyPosts;

public sealed class GetMyPostsHandler : IRequestHandler<GetMyPostsQuery, PagedResult<PostResult>>
{
    private readonly IPostRepository _postRepository;

    public GetMyPostsHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PagedResult<PostResult>> Handle(GetMyPostsQuery query, CancellationToken cancellationToken)
    {
        var pagedQuery = PagedQuery.FromPage(query.Page, query.PageSize);

        var (posts, total) = await _postRepository.GetPagedByAuthorIdAsync(query.UserId, pagedQuery, cancellationToken);

        var items = posts.Select(post => new PostResult
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
            CreatedAt = post.CreatedAt,
        }).ToList();

        return new PagedResult<PostResult>(total, items);
    }
}
