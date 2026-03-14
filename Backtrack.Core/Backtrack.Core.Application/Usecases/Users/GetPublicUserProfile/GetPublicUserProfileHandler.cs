using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Users.GetPublicUserProfile;

public sealed class GetPublicUserProfileHandler(IUserRepository userRepository)
    : IRequestHandler<GetPublicUserProfileQuery, PublicUserProfileResult>
{
    public async Task<PublicUserProfileResult> Handle(GetPublicUserProfileQuery query, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(query.UserId)
            ?? throw new NotFoundException(UserErrors.NotFound);

        return new PublicUserProfileResult
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Email = user.ShowEmail ? user.Email : null,
            Phone = user.ShowPhone ? user.Phone : null,
        };
    }
}
