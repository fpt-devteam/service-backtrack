using Backtrack.Core.Domain.Constants;
using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Posts.CreatePost;

public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.PostTitle)
            .NotEmpty().WithMessage("PostTitle is required")
            .MaximumLength(200).WithMessage("PostTitle must not exceed 200 characters");

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

        When(x => string.Equals(x.Category, "PersonalBelongings", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.PersonalBelongingDetail)
                .NotNull().WithMessage("PersonalBelongingDetail is required for category PersonalBelongings");
            When(x => x.PersonalBelongingDetail != null, () =>
            {
                RuleFor(x => x.PersonalBelongingDetail!.ItemName)
                    .NotEmpty().WithMessage("PersonalBelongingDetail.ItemName is required");
            });
        });

        When(x => string.Equals(x.Category, "Cards", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.CardDetail)
                .NotNull().WithMessage("CardDetail is required for category Cards");
            When(x => x.CardDetail != null, () =>
            {
                RuleFor(x => x.CardDetail!.ItemName)
                    .NotEmpty().WithMessage("CardDetail.ItemName is required");
            });
        });

        When(x => string.Equals(x.Category, "Electronics", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.ElectronicDetail)
                .NotNull().WithMessage("ElectronicDetail is required for category Electronics");
            When(x => x.ElectronicDetail != null, () =>
            {
                RuleFor(x => x.ElectronicDetail!.ItemName)
                    .NotEmpty().WithMessage("ElectronicDetail.ItemName is required");
            });
        });

        When(x => string.Equals(x.Category, "Others", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.OtherDetail)
                .NotNull().WithMessage("OtherDetail is required for category Others");
            When(x => x.OtherDetail != null, () =>
            {
                RuleFor(x => x.OtherDetail!.ItemName)
                    .NotEmpty().WithMessage("OtherDetail.ItemName is required");
            });
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
