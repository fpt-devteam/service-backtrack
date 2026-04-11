using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.QrDesigns;

public sealed record QrDesignResult
{
    public required Guid Id { get; init; }
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
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
