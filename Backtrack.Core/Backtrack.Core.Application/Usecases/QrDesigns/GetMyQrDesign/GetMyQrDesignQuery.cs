using MediatR;

namespace Backtrack.Core.Application.Usecases.QrDesigns.GetMyQrDesign;

public sealed record GetMyQrDesignQuery : IRequest<QrDesignResult>
{
    public required string UserId { get; init; }
}
