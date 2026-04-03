using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Handovers.CreateP2PHandover;

public sealed record CreateP2PHandoverCommand : IRequest<HandoverResult>
{
    [JsonIgnore]
    public  required string FinderId { get; init; }
    public Guid? FinderPostId { get; init; }
    public Guid? OwnerPostId { get; init; }
    public string? OwnerId { get; init; }
}
