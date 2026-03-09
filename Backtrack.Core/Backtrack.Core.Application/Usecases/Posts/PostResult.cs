using Backtrack.Core.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Posts;

public sealed record PostResult
{
    public required Guid Id { get; init; }
    public AuthorResult? Author { get; init; }
    public required string PostType { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public required string[] ImageUrls { get; init; }
    public required GeoPoint Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public required DateTimeOffset EventTime { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}