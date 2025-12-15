using Backtrack.Core.Contract.Common;

namespace Backtrack.Core.Contract.Posts.Requests;

public sealed record GetPostsRequest : PagedRequest
{
    public string? PostType { get; init; }
    public string? SearchTerm { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? RadiusInKm { get; init; }
}
