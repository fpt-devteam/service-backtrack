using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Contract.Users.Responses;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Users.Queries.GetMe;

public sealed class GetMeHandler : IRequestHandler<GetMeQuery, UserResponse>
{
    private readonly IUserRepository _userRepository;

    public GetMeHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new DomainException(UserErrors.NotFound);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        };
    }
}
