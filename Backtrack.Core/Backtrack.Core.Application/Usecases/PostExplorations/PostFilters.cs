using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.PostExplorations;

public sealed record GeoFilter(GeoPoint Location, double RadiusInKm);

public sealed record TimeFilter(DateTimeOffset? From, DateTimeOffset? To);

public sealed record PostFilters
{
    public PostType? PostType { get; init; }
    public ItemCategory? Category { get; init; }
    public GeoFilter? Geo { get; init; }
    public TimeFilter? Time { get; init; }
    public PostStatus? Status { get; init; }
    public string? AuthorId { get; init; }
    public Guid? OrganizationId { get; init; }
}
