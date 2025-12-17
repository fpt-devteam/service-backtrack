using Backtrack.Core.Contract.Common;

namespace Backtrack.Core.Contract.Posts.Requests;

public sealed record SearchPostsBySemanticRequest : PagedRequest
{
    public required string SearchText { get; init; }
    public string? PostType { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? RadiusInKm { get; init; }
}
