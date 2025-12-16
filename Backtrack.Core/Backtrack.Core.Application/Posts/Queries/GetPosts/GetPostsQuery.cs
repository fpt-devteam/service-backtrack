using Backtrack.Core.Application.Common;
using Backtrack.Core.Application.Posts.Common;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Posts.Queries.GetPosts;

public sealed record GetPostsQuery(
    PagedQuery PagedQuery,
    PostType? PostType,
    string? SearchTerm,
    double? Latitude,
    double? Longitude,
    double? RadiusInKm) : IRequest<PagedResult<PostResult>>;
