using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetMyPosts;

public sealed record GetMyPostsQuery(string UserId) : IRequest<List<PostResult>>;
