using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Users.GetMe;

public sealed class GetMeHandler : IRequestHandler<GetMeQuery, UserResult>
{
    private readonly IUserRepository _userRepository;

    public GetMeHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResult> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
        {
            throw new NotFoundException(UserErrors.NotFound);
        }

        return new UserResult
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Phone = user.Phone,
            ShowEmail = user.ShowEmail,
            ShowPhone = user.ShowPhone,
            GlobalRole = user.GlobalRole,
            Status = user.Status,
        };
    }
}
