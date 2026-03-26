using Backtrack.Core.Application.Usecases.Posts;
using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Users.GetMyPosts;

public sealed record GetMyPostsQuery : IRequest<PagedResult<PostResult>>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
