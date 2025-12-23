using Backtrack.Core.Application.Common.Interfaces.Messaging;
using Backtrack.Core.Application.Events.Integration;
using Backtrack.Core.Application.Users.Common;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Users.Commands.CreateUser;

public sealed class CreateUserHandler : IRequestHandler<CreateUserCommand, UserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher;

    public CreateUserHandler(IUserRepository userRepository, IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<UserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByIdAsync(request.UserId);

        if (existingUser != null)
        {
            return new UserResult
            {
                Id = existingUser.Id,
                Email = existingUser.Email,
                DisplayName = existingUser.DisplayName
            };
        }

        var newUser = new User
        {
            Id = request.UserId,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Status = UserStatus.Active,
            Role = UserRole.User,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.CreateAsync(newUser);
        await _userRepository.SaveChangesAsync();

        // Publish user created event
        await _eventPublisher.PublishUserCreatedAsync(new UserCreatedIntegrationEvent
        {
            Id = newUser.Id,
            Email = newUser.Email,
            DisplayName = newUser.DisplayName,
            CreatedAt = newUser.CreatedAt,
            EventTimestamp = DateTimeOffset.UtcNow
        });

        return new UserResult
        {
            Id = newUser.Id,
            Email = newUser.Email,
            DisplayName = newUser.DisplayName
        };
    }
}
