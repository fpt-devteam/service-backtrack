using Backtrack.Core.Application.Common;
using Backtrack.Core.Application.Posts.Common;
using MediatR;

namespace Backtrack.Core.Application.Posts.Queries.GetPosts;

public sealed record GetPostsQuery(
    PagedQuery PagedQuery,
    string? PostType,
    string? SearchTerm,
    double? Latitude,
    double? Longitude,
    double? RadiusInKm) : IRequest<PagedResult<PostResult>>;
