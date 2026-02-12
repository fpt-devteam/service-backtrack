using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.CreateInvitation;

public sealed record CreateInvitationCommand : IRequest<CreateInvitationResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public string UserName { get; init; } = string.Empty;

    public required Guid OrgId { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }
}
