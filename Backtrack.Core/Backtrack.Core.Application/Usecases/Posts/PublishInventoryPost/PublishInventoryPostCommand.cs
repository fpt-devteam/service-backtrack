using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.PublishInventoryPost;

public sealed record PublishInventoryPostCommand : IRequest<PostResult>
{
    [JsonIgnore]
    public Guid PostId { get; init; }
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;
    [JsonIgnore]
    public Guid OrgId { get; init; }
}
