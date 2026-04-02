using Backtrack.Core.Domain.Constants;
using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Posts.CreatePost;

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Item)
            .NotNull().WithMessage("Item is required");

        When(x => x.Item != null, () =>
        {
            RuleFor(x => x.Item.ItemName)
                .NotEmpty().WithMessage("Item.ItemName is required")
                .MaximumLength(500).WithMessage("Item.ItemName must not exceed 500 characters");

            RuleFor(x => x.Item.Category)
                .IsInEnum().WithMessage($"Item.Category must be one of: {string.Join(", ", Enum.GetNames<ItemCategory>())}")
                .When(x => x.Item.Category != null);

            RuleFor(x => x.Item.AdditionalDetails)
                .MaximumLength(2000).WithMessage("Item.AdditionalDetails must not exceed 2000 characters");
        });

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

        RuleFor(x => x.ImageUrls)
            .NotEmpty().WithMessage("At least one image is required")
            .Must(images => images.Length <= 5).WithMessage("No more than 5 images are allowed");
    }
}
