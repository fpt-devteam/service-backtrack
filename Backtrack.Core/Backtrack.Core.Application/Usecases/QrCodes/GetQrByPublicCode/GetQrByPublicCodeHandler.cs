using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QrCodes.GetQrByPublicCode;

public sealed class GetQrByPublicCodeHandler(IQrCodeRepository qrCodeRepository)
    : IRequestHandler<GetQrByPublicCodeQuery, QrCodeResult>
{
    public async Task<QrCodeResult> Handle(GetQrByPublicCodeQuery query, CancellationToken cancellationToken)
    {
        var qrCode = await qrCodeRepository.GetByPublicCodeAsync(query.PublicCode, cancellationToken)
            ?? throw new NotFoundException(QrErrors.NotFound);

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
