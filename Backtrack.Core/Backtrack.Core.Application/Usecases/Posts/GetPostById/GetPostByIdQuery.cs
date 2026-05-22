using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Posts.GetPostById;

public sealed record GetPostByIdQuery : IRequest<PostResult>
{
    [JsonIgnore]
    public string? UserId { get; init; }
    public required Guid PostId { get; init; }
    public bool IsBlurImages { get; init; } = true;
}
