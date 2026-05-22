using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.QrCodes.UpdateQrCode;

public sealed record UpdateQrCodeCommand : IRequest<QrCodeResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;
    public string? Note { get; init; }
    public string? LogoUrl { get; init; }
    public bool? ShowEmail { get; init; }
    public bool? ShowPhone { get; init; }
}
