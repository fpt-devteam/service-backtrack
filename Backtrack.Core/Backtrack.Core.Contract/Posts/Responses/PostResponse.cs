namespace Backtrack.Core.Contract.Posts.Responses;

public sealed record PostResponse
{
    public required Guid Id { get; init; }
    public required string PostType { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public required string[] Material { get; init; }
    public required string[] Brands { get; init; }
    public required string[] Colors { get; init; }
    public required string[] ImageUrls { get; init; }
    public LocationResponse? Location { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public double? DistanceInKm { get; init; }
}

public sealed record LocationResponse
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
