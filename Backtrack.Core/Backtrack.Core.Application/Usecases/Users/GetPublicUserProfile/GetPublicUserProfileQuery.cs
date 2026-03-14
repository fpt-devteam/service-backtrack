using MediatR;

namespace Backtrack.Core.Application.Usecases.Users.GetPublicUserProfile;

public sealed record GetPublicUserProfileQuery : IRequest<PublicUserProfileResult>
{
    public required string UserId { get; init; }
}
