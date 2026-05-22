using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Usecases.QrCodes;

public sealed record QrCodeResult
{
    public required Guid Id { get; init; }
    public required string UserId { get; init; }
    public required string PublicCode { get; init; }
    public required string Note { get; init; }
    public string? LogoUrl { get; init; }
    public bool? ShowEmail { get; init; }
    public bool? ShowPhone { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public static class QrCodeResultMapper
{
    public static QrCodeResult ToQrCodeResult(this QrCode qrCode, bool? showEmail = null, bool? showPhone = null) => new()
    {
        Id = qrCode.Id,
        UserId = qrCode.UserId,
        PublicCode = qrCode.PublicCode,
        Note = qrCode.Note,
        LogoUrl = qrCode.LogoUrl,
        ShowEmail = showEmail,
        ShowPhone = showPhone,
        CreatedAt = qrCode.CreatedAt,
        UpdatedAt = qrCode.UpdatedAt,
    };
}
