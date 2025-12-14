using Backtrack.Core.Contract.Users.Responses;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Users.Commands.CreateUser;

public sealed class CreateUserHandler : IRequestHandler<CreateUserCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;

    public CreateUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByIdAsync(request.Request.UserId);

        if (existingUser != null)
        {
            return new UserResponse
            {
                Id = existingUser.Id,
                Email = existingUser.Email,
                DisplayName = existingUser.DisplayName
            };
        }

        var newUser = new User
        {
            Id = request.Request.UserId,
            Email = request.Request.Email,
            DisplayName = request.Request.DisplayName,
            Status = UserStatus.Active,
            Role = UserRole.User,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.CreateAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return new UserResponse
        {
            Id = newUser.Id,
            Email = newUser.Email,
            DisplayName = newUser.DisplayName
        };
    }
}
