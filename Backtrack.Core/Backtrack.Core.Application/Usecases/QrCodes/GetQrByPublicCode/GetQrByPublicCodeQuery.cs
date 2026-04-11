using MediatR;

namespace Backtrack.Core.Application.Usecases.QrCodes.GetQrByPublicCode;

public sealed record GetQrByPublicCodeQuery : IRequest<QrPublicResult>
{
    public required string PublicCode { get; init; }
}
