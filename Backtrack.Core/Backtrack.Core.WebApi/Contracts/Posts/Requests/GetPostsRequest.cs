
using Backtrack.Core.WebApi.Contracts.Common;

namespace Backtrack.Core.WebApi.Contracts.Posts.Requests;

public sealed record GetPostsRequest : PagedRequest
{
    public string? PostType { get; init; }
    public string? SearchTerm { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? RadiusInKm { get; init; }
    public string? AuthorId { get; init; }
}
