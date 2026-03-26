using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Backtrack.Core.Application.Usecases;

namespace Backtrack.Core.Application.Usecases.Posts.GetFeed;

public sealed record GetFeedQuery : IRequest<PagedResult<FeedPostResult>>
{
    public required GeoPoint Location { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
