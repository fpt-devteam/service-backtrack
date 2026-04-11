using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Users.EnsureUserExist;

public sealed class EnsureUserExistHandler : IRequestHandler<EnsureUserExistCommand, UserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly IEventPublisher _eventPublisher;

    public EnsureUserExistHandler(
        IUserRepository userRepository,
        IQrCodeRepository qrCodeRepository,
        IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _qrCodeRepository = qrCodeRepository;
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

        // Provision a QR code for the user if one doesn't exist yet
        var existingQr = await _qrCodeRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (existingQr is null)
        {
            string publicCode;
            do { publicCode = QrCodeUtil.GeneratePublicCode(); }
            while (await _qrCodeRepository.PublicCodeExistsAsync(publicCode, cancellationToken));

            await _qrCodeRepository.CreateAsync(new QrCode
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                PublicCode = publicCode,
                Note = $"Hi! I'm {request.DisplayName ?? request.Email}. If you found something that belongs to me, please reach out — I'd really appreciate it!",
            });
            await _qrCodeRepository.SaveChangesAsync();
        }

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
            Phone = existingOrCreatedUser.Phone,
            ShowEmail = existingOrCreatedUser.ShowEmail,
            ShowPhone = existingOrCreatedUser.ShowPhone,
            GlobalRole = existingOrCreatedUser.GlobalRole,
            Status = existingOrCreatedUser.Status,
        };
    }
}
