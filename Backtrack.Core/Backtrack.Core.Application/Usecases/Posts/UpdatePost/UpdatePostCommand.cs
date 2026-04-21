using Backtrack.Core.Application.Usecases.Posts.CreatePost;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.UpdatePost;

public sealed record UpdatePostCommand : IRequest<PostResult>
{
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid PostId { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string UserId { get; init; } = string.Empty;
    public Guid? OrganizationId { get; init; }
    public string? PostTitle { get; init; }
    public string? PostType { get; init; }
    public string? Status { get; init; }
    public PersonalBelongingDetailInput? PersonalBelongingDetail { get; init; }
    public CardDetailInput? CardDetail { get; init; }
    public ElectronicDetailInput? ElectronicDetail { get; init; }
    public OtherDetailInput? OtherDetail { get; init; }
    public GeoPoint? Location { get; init; }
    public string? ExternalPlaceId { get; init; }
    public string? DisplayAddress { get; init; }
    public string[]? ImageUrls { get; init; }
    public DateTimeOffset? EventTime { get; init; }
}
