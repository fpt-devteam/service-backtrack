using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.JoinByInvitation;

public sealed record JoinByInvitationCommand : IRequest<JoinByInvitationResult>
{
    public required string Token { get; init; }

    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public string UserEmail { get; init; } = string.Empty;
}
