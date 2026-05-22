using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Users.UpdateUserProfile;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QrCodes.UpdateQrCode;

public sealed class UpdateQrCodeHandler(
    IQrCodeRepository qrCodeRepository,
    IUserRepository userRepository,
    ISender sender)
    : IRequestHandler<UpdateQrCodeCommand, QrCodeResult>
{
    public async Task<QrCodeResult> Handle(UpdateQrCodeCommand command, CancellationToken cancellationToken)
    {
        var qrCode = await qrCodeRepository.GetByUserIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException(QrErrors.NotFound);

        if (command.Note is not null)
            qrCode.Note = command.Note;
        if (command.LogoUrl is not null)
            qrCode.LogoUrl = command.LogoUrl;
        qrCode.UpdatedAt = DateTimeOffset.UtcNow;
        qrCodeRepository.Update(qrCode);
        await qrCodeRepository.SaveChangesAsync();

        if (command.ShowEmail.HasValue || command.ShowPhone.HasValue)
        {
            await sender.Send(new UpdateUserProfileCommand
            {
                UserId = command.UserId,
                ShowEmail = command.ShowEmail,
                ShowPhone = command.ShowPhone,
            }, cancellationToken);
        }

        var user = await userRepository.GetByIdAsync(command.UserId);
        return qrCode.ToQrCodeResult(user?.ShowEmail, user?.ShowPhone);
    }
}
