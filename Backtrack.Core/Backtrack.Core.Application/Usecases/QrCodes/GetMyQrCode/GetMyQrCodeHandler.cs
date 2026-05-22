using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QrCodes.GetMyQrCode;

public sealed class GetMyQrCodeHandler(IQrCodeRepository qrCodeRepository, IUserRepository userRepository)
    : IRequestHandler<GetMyQrCodeQuery, QrCodeResult>
{
    public async Task<QrCodeResult> Handle(GetMyQrCodeQuery query, CancellationToken cancellationToken)
    {
        var qrCode = await qrCodeRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        if (qrCode is null)
        {
            string publicCode;
            do { publicCode = QrCodeUtil.GeneratePublicCode(); }
            while (await qrCodeRepository.PublicCodeExistsAsync(publicCode, cancellationToken));

            qrCode = await qrCodeRepository.CreateAsync(new QrCode
            {
                Id = Guid.NewGuid(),
                UserId = query.UserId,
                PublicCode = publicCode,
                Note = "If you found something that belongs to me, please reach out — I'd really appreciate it!",
            });
            await qrCodeRepository.SaveChangesAsync();
        }

        var user = await userRepository.GetByIdAsync(query.UserId);
        return qrCode.ToQrCodeResult(user?.ShowEmail, user?.ShowPhone);
    }
}
