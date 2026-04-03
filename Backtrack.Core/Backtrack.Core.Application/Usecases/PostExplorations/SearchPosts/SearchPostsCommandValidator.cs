using FluentValidation;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SearchPosts;

public sealed class SearchPostsCommandValidator : AbstractValidator<SearchPostsCommand>
{
    public SearchPostsCommandValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query is required")
            .MaximumLength(500).WithMessage("Query must not exceed 500 characters");

        When(x => x.Filters?.Geo != null, () =>
        {
            RuleFor(x => x.Filters!.Geo!.Location.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Filters!.Geo!.Location.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

            RuleFor(x => x.Filters!.Geo!.RadiusInKm)
                .InclusiveBetween(1, 20).WithMessage("RadiusInKm must be between 1 and 20");
        });

        When(x => x.Filters?.Time != null, () =>
        {
            RuleFor(x => x.Filters!.Time!.To)
                .GreaterThan(x => x.Filters!.Time!.From)
                .WithMessage("Time.To must be after Time.From");
        });
    }
}
