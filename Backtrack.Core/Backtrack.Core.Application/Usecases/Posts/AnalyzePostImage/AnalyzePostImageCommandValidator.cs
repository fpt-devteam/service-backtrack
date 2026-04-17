using Backtrack.Core.Domain.Constants;
using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Posts.AnalyzePostImage;

public sealed class AnalyzePostImageCommandValidator : AbstractValidator<AnalyzePostImageCommand>
{
    public AnalyzePostImageCommandValidator()
    {
        RuleFor(x => x.ImageUrls)
            .NotEmpty().WithMessage("ImageUrls is required");

        RuleForEach(x => x.ImageUrls)
            .NotEmpty().WithMessage("ImageUrl must not be empty")
            .Must(BeValidUrl).WithMessage("Each ImageUrl must be a valid absolute HTTP/HTTPS URI");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(BeValidCategory)
            .WithMessage($"Category must be one of: {string.Join(", ", Enum.GetNames<ItemCategory>())}");
    }

    private static bool BeValidUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static bool BeValidCategory(string category) =>
        Enum.TryParse<ItemCategory>(category, ignoreCase: true, out _);
}
