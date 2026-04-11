using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Domain.Entities;

public sealed class QrDesign : Entity<Guid>
{
    public required string UserId { get; set; }

    // Colors
    public required string ForegroundColor { get; set; }
    public required string BackgroundColor { get; set; }

    // Dot style
    public required string DotStyle { get; set; } // "square" | "rounded" | "dots" | "classy" | "classy-rounded" | "extra-rounded"

    // Corner square style
    public required string CornerSquareStyle { get; set; } // "square" | "dot" | "extra-rounded"
    public required string CornerSquareColor { get; set; }

    // Corner dot style
    public required string CornerDotStyle { get; set; } // "square" | "dot"
    public required string CornerDotColor { get; set; }

    // Error correction level
    public required QrEcl ErrorCorrectionLevel { get; set; }

    // Logo (optional)
    public QrLogoSettings? Logo { get; set; }

    // Gradient (optional)
    public QrGradientSettings? Gradient { get; set; }
}
