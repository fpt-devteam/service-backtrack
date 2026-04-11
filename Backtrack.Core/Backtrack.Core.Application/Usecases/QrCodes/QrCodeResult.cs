using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.QrCodes;

public sealed record QrCodeResult
{
    public required Guid Id { get; init; }
    public required string UserId { get; init; }
    public required string PublicCode { get; init; }
    public string? Note { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
