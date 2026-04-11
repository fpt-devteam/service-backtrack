using MediatR;

namespace Backtrack.Core.Application.Usecases.QrCodes.GetMyQrCode;

public sealed record GetMyQrCodeQuery : IRequest<QrCodeResult>
{
    public required string UserId { get; init; }
}
