using Backtrack.Core.Application.Common.Interfaces.Messaging;
using Backtrack.Core.Application.Events.Integration;
using Backtrack.Core.Application.Users.Common;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Users.Commands.UpsertUser;

public sealed class UpsertUserHandler : IRequestHandler<UpsertUserCommand, UserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher;

    public UpsertUserHandler(IUserRepository userRepository, IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<UserResult> Handle(UpsertUserCommand request, CancellationToken cancellationToken)
    {
        // Upsert user (create if not exists, update if exists)
        var user = new User
        {
            Id = request.UserId,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Status = UserStatus.Active,
            GlobalRole = UserGlobalRole.Customer
        };

        var upsertedUser = await _userRepository.UpsertAsync(user);

        // Publish user upserted event
        await _eventPublisher.PublishUserUpsertedAsync(new UserUpsertedIntegrationEvent
        {
            Id = upsertedUser.Id,
            Email = upsertedUser.Email,
            DisplayName = upsertedUser.DisplayName,
            GlobalRole = upsertedUser.GlobalRole.ToString(),
            CreatedAt = upsertedUser.CreatedAt,
            EventTimestamp = DateTimeOffset.UtcNow
        });

        return new UserResult
        {
            Id = upsertedUser.Id,
            Email = upsertedUser.Email,
            DisplayName = upsertedUser.DisplayName,
            GlobalRole = upsertedUser.GlobalRole
        };
    }
}
