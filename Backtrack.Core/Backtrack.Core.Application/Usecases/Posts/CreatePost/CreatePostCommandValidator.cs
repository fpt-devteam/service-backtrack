using Backtrack.Core.Domain.Constants;
using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Posts.CreatePost;

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage("ItemName is required")
            .MaximumLength(500).WithMessage("ItemName must not exceed 500 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.EventTime)
            .NotEmpty().WithMessage("EventTime is required");

        RuleFor(x => x.Location)
            .NotNull().WithMessage("Location is required");

        RuleFor(x => x.DisplayAddress)
            .NotEmpty().WithMessage("DisplayAddress is required")
            .MaximumLength(1000).WithMessage("DisplayAddress must not exceed 1000 characters");

        RuleFor(x => x.Location.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Location.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.Images)
            .NotEmpty().WithMessage("At least one image is required")
            .Must(images => images.Length <= 5).WithMessage("No more than 5 images are allowed");

        RuleForEach(x => x.Images).ChildRules(image =>
        {
            image.RuleFor(i => i.Url)
                .NotEmpty().WithMessage("Image URL is required")
                .MaximumLength(2048).WithMessage("Image URL must not exceed 2048 characters");

            image.RuleFor(i => i.Base64Data)
                .NotEmpty().WithMessage("Image Base64Data is required");

            image.RuleFor(i => i.MimeType)
                .NotEmpty().WithMessage("Image MimeType is required")
                .Must(m => m == "image/png" || m == "image/jpeg")
                .WithMessage("Image MimeType must be image/png or image/jpeg");

            image.RuleFor(i => i.FileName)
                .MaximumLength(255).WithMessage("Image FileName must not exceed 255 characters")
                .When(i => i.FileName is not null);

            image.RuleFor(i => i.FileSizeBytes)
                .GreaterThan(0).WithMessage("Image FileSizeBytes must be greater than 0")
                .When(i => i.FileSizeBytes.HasValue);
        });
    }
}
