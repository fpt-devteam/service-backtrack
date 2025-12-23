using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Common.Exceptions.Errors;
using Backtrack.Core.Application.Common.Interfaces.Messaging;
using Backtrack.Core.Application.Events.Integration;
using Backtrack.Core.Application.Users.Common;
using MediatR;

namespace Backtrack.Core.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher;

    public UpdateUserHandler(IUserRepository userRepository, IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<UserResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
        {
            throw new NotFoundException(UserErrors.NotFound);
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.Email = request.Email;
        }

        if (request.DisplayName != null)
        {
            user.DisplayName = request.DisplayName;
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        // Publish user updated event
        await _eventPublisher.PublishUserUpdatedAsync(new UserUpdatedIntegrationEvent
        {
            Id = user.Id,
            Email = request.Email,
            DisplayName = request.DisplayName,
            UpdatedAt = user.UpdatedAt.Value,
            EventTimestamp = DateTimeOffset.UtcNow
        });

        return new UserResult
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        };
    }
}
