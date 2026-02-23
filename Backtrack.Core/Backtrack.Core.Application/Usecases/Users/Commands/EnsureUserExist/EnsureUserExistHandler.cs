using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Users.Commands.EnsureUserExist;

public sealed class EnsureUserExistHandler : IRequestHandler<EnsureUserExistCommand, UserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher;

    public EnsureUserExistHandler(IUserRepository userRepository, IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<UserResult> Handle(EnsureUserExistCommand request, CancellationToken cancellationToken)
    {
        // Ensure user exists (create if not exists, return existing if exists)
        var user = new User
        {
            Id = request.UserId,
            Email = request.Email,
            DisplayName = request.DisplayName,
            AvatarUrl = request.AvatarUrl,
            Status = UserStatus.Active,
            GlobalRole = UserGlobalRole.Customer
        };

        var existingOrCreatedUser = await _userRepository.EnsureExistAsync(user);

        // Publish user ensure exist event
        await _eventPublisher.PublishUserEnsureExistAsync(new UserEnsureExistIntegrationEvent
        {
            Id = existingOrCreatedUser.Id,
            Email = existingOrCreatedUser.Email,
            DisplayName = existingOrCreatedUser.DisplayName,
            AvatarUrl = existingOrCreatedUser.AvatarUrl,
            GlobalRole = existingOrCreatedUser.GlobalRole.ToString(),
            CreatedAt = existingOrCreatedUser.CreatedAt,
            EventTimestamp = DateTimeOffset.UtcNow
        });

        return new UserResult
        {
            Id = existingOrCreatedUser.Id,
            Email = existingOrCreatedUser.Email,
            DisplayName = existingOrCreatedUser.DisplayName,
            AvatarUrl = existingOrCreatedUser.AvatarUrl,
            GlobalRole = existingOrCreatedUser.GlobalRole
        };
    }
}
