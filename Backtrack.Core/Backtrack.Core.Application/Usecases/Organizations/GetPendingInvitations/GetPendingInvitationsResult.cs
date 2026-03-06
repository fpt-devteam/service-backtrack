namespace Backtrack.Core.Application.Usecases.Organizations.GetPendingInvitations;

public sealed record GetPendingInvitationsResult
{
    public required IEnumerable<InvitationResponse> Invitations { get; init; }
}

public sealed record InvitationResponse
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset ExpiredTime { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
