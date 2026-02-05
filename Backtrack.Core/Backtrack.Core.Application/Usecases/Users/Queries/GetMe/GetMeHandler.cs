using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Users.Queries.GetMe;

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
            GlobalRole = user.GlobalRole
        };
    }
}
