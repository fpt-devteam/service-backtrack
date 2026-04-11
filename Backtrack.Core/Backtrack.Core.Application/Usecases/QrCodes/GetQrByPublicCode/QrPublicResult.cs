namespace Backtrack.Core.Application.Usecases.QrCodes.GetQrByPublicCode;

public sealed record QrPublicResult
{
    public required string PublicCode { get; init; }
    public required string Note { get; init; }

    // Owner info — nullable fields respect ShowEmail / ShowPhone
    public required string? DisplayName { get; init; }
    public required string? AvatarUrl { get; init; }
    public required string? Email { get; init; }
    public required string? Phone { get; init; }
}
