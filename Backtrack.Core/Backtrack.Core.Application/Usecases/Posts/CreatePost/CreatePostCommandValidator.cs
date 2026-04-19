using Backtrack.Core.Domain.Constants;
using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Posts.CreatePost;

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(c => Enum.TryParse<ItemCategory>(c, ignoreCase: true, out _))
            .WithMessage($"Category must be one of: {string.Join(", ", Enum.GetNames<ItemCategory>())}");

        RuleFor(x => x.SubcategoryCode)
            .NotEmpty().WithMessage("SubcategoryCode is required");

        RuleFor(x => x.ImageUrls)
            .Must(images => images.Length <= 5).WithMessage("No more than 5 images are allowed");

        Unless(x => string.Equals(x.PostType, "Lost", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.ImageUrls)
                .NotEmpty().WithMessage("At least one image is required");
        });

        // Location, DisplayAddress, EventTime, PostType are required only for non-org posts
        When(x => !x.OrganizationId.HasValue, () =>
        {
            RuleFor(x => x.PostType)
                .NotEmpty().WithMessage("PostType is required")
                .Must(t => Enum.TryParse<PostType>(t, ignoreCase: true, out _))
                .WithMessage($"PostType must be one of: {string.Join(", ", Enum.GetNames<PostType>())}");

            RuleFor(x => x.Location)
                .NotNull().WithMessage("Location is required");

            RuleFor(x => x.DisplayAddress)
                .NotEmpty().WithMessage("DisplayAddress is required")
                .MaximumLength(1000).WithMessage("DisplayAddress must not exceed 1000 characters");

            RuleFor(x => x.EventTime)
                .NotEmpty().WithMessage("EventTime is required");

            When(x => x.Location != null, () =>
            {
                RuleFor(x => x.Location!.Latitude)
                    .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

                RuleFor(x => x.Location!.Longitude)
                    .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");
            });
        });
    }
}
