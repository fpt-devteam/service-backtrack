using FluentValidation;

namespace Backtrack.Core.Application.ImageAnalysis.Commands.AnalyzeImage;

public sealed class AnalyzeImageCommandValidator : AbstractValidator<AnalyzeImageCommand>
{
    private static readonly string[] AllowedMimeTypes = new[]
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    public AnalyzeImageCommandValidator()
    {
        RuleFor(x => x.ImageBase64)
            .NotEmpty().WithMessage("ImageBase64 is required")
            .Must(BeValidBase64).WithMessage("ImageBase64 must be valid base64 encoded data");

        RuleFor(x => x.MimeType)
            .NotEmpty().WithMessage("MimeType is required")
            .Must(BeAllowedMimeType).WithMessage($"MimeType must be one of: {string.Join(", ", AllowedMimeTypes)}");
    }

    private static bool BeValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return false;

        try
        {
            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool BeAllowedMimeType(string mimeType)
    {
        return AllowedMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase);
    }
}
