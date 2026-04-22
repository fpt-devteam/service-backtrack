using Backtrack.Core.Application.Usecases;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostExplorations.ListPostByFeed;

public sealed record ListPostByFeedQuery : IRequest<ListPostByFeedResult>
{
    public required GeoPoint Location { get; init; }
    public PostType? PostType { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
