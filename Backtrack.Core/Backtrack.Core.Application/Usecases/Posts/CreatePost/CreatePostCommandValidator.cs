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
            .NotNull().WithMessage("Location is required when not posting as an organization")
            .When(x => x.OrganizationId == null);

        RuleFor(x => x.DisplayAddress)
            .NotEmpty().WithMessage("DisplayAddress is required when not posting as an organization")
            .When(x => x.OrganizationId == null);

        RuleFor(x => x.Location!.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.Location != null);

        RuleFor(x => x.Location!.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.Location != null);
    }
}
