using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QrCodes.GetMyQrCode;

public sealed class GetMyQrCodeHandler(IQrCodeRepository qrCodeRepository)
    : IRequestHandler<GetMyQrCodeQuery, QrCodeResult>
{
    public async Task<QrCodeResult> Handle(GetMyQrCodeQuery query, CancellationToken cancellationToken)
    {
        var qrCode = await qrCodeRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        if (qrCode is null)
        {
            // Auto-provision a QR code on first access
            string publicCode;
            do { publicCode = QrCodeUtil.GeneratePublicCode(); }
            while (await qrCodeRepository.PublicCodeExistsAsync(publicCode, cancellationToken));

            qrCode = await qrCodeRepository.CreateAsync(new QrCode
            {
                Id = Guid.NewGuid(),
                UserId = query.UserId,
                PublicCode = publicCode,
            });
            await qrCodeRepository.SaveChangesAsync();
        }

        return MapToResult(qrCode);
    }

    private static QrCodeResult MapToResult(QrCode qrCode) => new()
    {
        Id = qrCode.Id,
        UserId = qrCode.UserId,
        PublicCode = qrCode.PublicCode,
        Note = qrCode.Note,
        CreatedAt = qrCode.CreatedAt,
        UpdatedAt = qrCode.UpdatedAt,
    };
}
