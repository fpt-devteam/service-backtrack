using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Backtrack.Core.Application.Usecases.Posts.CreatePost;

public sealed record CreatePostCommand : IRequest<PostResult>
{
    public string AuthorId { get; init; } = string.Empty;
    public Guid? OrganizationId { get; init; }
    public required string PostType { get; init; }
    public required string ItemName { get; init; }
    public required string Description { get; init; }
    public string? DistinctiveMarks { get; init; }
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public GeoPoint? Location { get; init; }
    public string? DisplayAddress { get; init; }
    public string? ExternalPlaceId { get; init; }
    public required DateTimeOffset EventTime { get; init; }
}