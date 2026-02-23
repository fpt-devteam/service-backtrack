using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Queries.CheckInvitation;

public sealed record CheckInvitationQuery : IRequest<CheckInvitationResult>
{
    public required string Token { get; init; }
    public required string Email { get; init; }
}
