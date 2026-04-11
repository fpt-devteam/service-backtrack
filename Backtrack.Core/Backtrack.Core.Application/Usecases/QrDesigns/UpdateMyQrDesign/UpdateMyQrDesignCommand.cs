using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.QrDesigns.UpdateMyQrDesign;

public sealed record UpdateMyQrDesignCommand : IRequest<QrDesignResult>
{
    public required string UserId { get; init; }
    public required string ForegroundColor { get; init; }
    public required string BackgroundColor { get; init; }
    public required string DotStyle { get; init; }
    public required string CornerSquareStyle { get; init; }
    public required string CornerSquareColor { get; init; }
    public required string CornerDotStyle { get; init; }
    public required string CornerDotColor { get; init; }
    public required QrEcl ErrorCorrectionLevel { get; init; }
    public QrLogoSettings? Logo { get; init; }
    public QrGradientSettings? Gradient { get; init; }
}
