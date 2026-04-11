using MediatR;

namespace Backtrack.Core.Application.Usecases.QrCodes.UpdateQrNote;

public sealed record UpdateQrNoteCommand : IRequest<QrCodeResult>
{
    public required string UserId { get; init; }
    public string? Note { get; init; }
}
