using FluentValidation;

namespace Backtrack.Core.Application.Usecases.PostExplorations.ListPostByFeed;

public sealed class ListPostByFeedQueryValidator : AbstractValidator<ListPostByFeedQuery>
{
    public ListPostByFeedQueryValidator()
    {
        RuleFor(x => x.Location)
            .NotNull()
            .WithMessage("Location is required");

        When(x => x.Location != null, () =>
        {
            RuleFor(x => x.Location.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Location.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Longitude must be between -180 and 180");
        });

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}
