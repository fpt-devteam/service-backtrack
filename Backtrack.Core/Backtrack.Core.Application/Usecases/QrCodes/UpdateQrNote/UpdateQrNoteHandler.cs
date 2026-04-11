using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QrCodes.UpdateQrNote;

public sealed class UpdateQrNoteHandler(IQrCodeRepository qrCodeRepository)
    : IRequestHandler<UpdateQrNoteCommand, QrCodeResult>
{
    public async Task<QrCodeResult> Handle(UpdateQrNoteCommand command, CancellationToken cancellationToken)
    {
        var qrCode = await qrCodeRepository.GetByUserIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException(QrErrors.NotFound);

        qrCode.Note = command.Note;
        qrCodeRepository.Update(qrCode);
        await qrCodeRepository.SaveChangesAsync();

        return new QrCodeResult
        {
            Id = qrCode.Id,
            UserId = qrCode.UserId,
            PublicCode = qrCode.PublicCode,
            Note = qrCode.Note,
            CreatedAt = qrCode.CreatedAt,
            UpdatedAt = qrCode.UpdatedAt,
        };
    }
}
