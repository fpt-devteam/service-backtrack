using MediatR;

namespace Backtrack.Core.Application.Usecases.QrCodes.GetQrByPublicCode;

public sealed record GetQrByPublicCodeQuery : IRequest<QrCodeResult>
{
    public required string PublicCode { get; init; }
}
