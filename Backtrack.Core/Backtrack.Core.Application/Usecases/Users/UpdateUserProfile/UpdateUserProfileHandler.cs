using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Users.UpdateUserProfile;

public sealed class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, UserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher;

    public UpdateUserProfileHandler(IUserRepository userRepository, IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<UserResult> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, isTrack: true);

        if (user == null)
            throw new NotFoundException(UserErrors.NotFound);

        if (request.DisplayName != null)
            user.DisplayName = request.DisplayName;

        if (request.AvatarUrl != null)
            user.AvatarUrl = request.AvatarUrl;

        if (request.Phone != null)
            user.Phone = request.Phone;

        if (request.ShowEmail.HasValue)
            user.ShowEmail = request.ShowEmail.Value;

        if (request.ShowPhone.HasValue)
            user.ShowPhone = request.ShowPhone.Value;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        await _eventPublisher.PublishUserEnsureExistAsync(new UserEnsureExistIntegrationEvent
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            GlobalRole = user.GlobalRole.ToString(),
            CreatedAt = user.CreatedAt,
            EventTimestamp = DateTimeOffset.UtcNow,
        });

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
