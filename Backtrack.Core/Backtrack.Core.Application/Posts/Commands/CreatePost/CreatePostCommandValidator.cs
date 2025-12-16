using Backtrack.Core.Domain.Constants;
using FluentValidation;

namespace Backtrack.Core.Application.Posts.Commands.CreatePost;

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

        RuleFor(x => x.Location!.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.Location != null);

        RuleFor(x => x.Location!.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.Location != null);

        // Ensure all location fields are either all null or all not null
        RuleFor(x => x.ExternalPlaceId)
            .NotEmpty().WithMessage("ExternalPlaceId is required when Location is provided")
            .When(x => x.Location != null);

        RuleFor(x => x.ExternalPlaceId)
            .Must(BeNull).WithMessage("ExternalPlaceId must be null when Location is not provided")
            .When(x => x.Location == null);

        RuleFor(x => x.DisplayAddress)
            .NotEmpty().WithMessage("DisplayAddress is required when Location is provided")
            .When(x => x.Location != null);

        RuleFor(x => x.DisplayAddress)
            .Must(BeNull).WithMessage("DisplayAddress must be null when Location is not provided")
            .When(x => x.Location == null);
    }

    private static bool BeNull(string? value) => string.IsNullOrWhiteSpace(value);
}
