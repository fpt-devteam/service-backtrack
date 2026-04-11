using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QrCodes.GetQrByPublicCode;

public sealed class GetQrByPublicCodeHandler(
    IQrCodeRepository qrCodeRepository,
    IUserRepository userRepository)
    : IRequestHandler<GetQrByPublicCodeQuery, QrPublicResult>
{
    public async Task<QrPublicResult> Handle(GetQrByPublicCodeQuery query, CancellationToken cancellationToken)
    {
        var qrCode = await qrCodeRepository.GetByPublicCodeAsync(query.PublicCode, cancellationToken)
            ?? throw new NotFoundException(QrErrors.NotFound);

        var user = await userRepository.GetByIdAsync(qrCode.UserId)
            ?? throw new NotFoundException(UserErrors.NotFound);

        return new QrPublicResult
        {
            PublicCode = qrCode.PublicCode,
            Note = qrCode.Note,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            Email = user.ShowEmail ? user.Email : null,
            Phone = user.ShowPhone ? user.Phone : null,
        };
    }
}
