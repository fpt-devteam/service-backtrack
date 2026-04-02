using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Posts.AnalyzePostImage;

public sealed class AnalyzePostImageCommandValidator : AbstractValidator<AnalyzePostImageCommand>
{
    public AnalyzePostImageCommandValidator()
    {
        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("ImageUrl is required")
            .Must(BeValidUrl).WithMessage("ImageUrl must be a valid absolute URI");
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
