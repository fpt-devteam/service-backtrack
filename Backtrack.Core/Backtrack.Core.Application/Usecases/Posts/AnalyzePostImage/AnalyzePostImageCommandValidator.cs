using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Posts.AnalyzePostImage;

public sealed class AnalyzePostImageCommandValidator : AbstractValidator<AnalyzePostImageCommand>
{
    public AnalyzePostImageCommandValidator()
    {
        RuleFor(x => x.SubcategoryCode)
            .NotEmpty().WithMessage("SubcategoryCode is required");

        RuleFor(x => x.ImageUrls)
            .NotEmpty().WithMessage("ImageUrls is required");

        RuleForEach(x => x.ImageUrls)
            .NotEmpty().WithMessage("ImageUrl must not be empty")
            .Must(BeValidUrl).WithMessage("Each ImageUrl must be a valid absolute HTTP/HTTPS URI");
    }

    private static bool BeValidUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
