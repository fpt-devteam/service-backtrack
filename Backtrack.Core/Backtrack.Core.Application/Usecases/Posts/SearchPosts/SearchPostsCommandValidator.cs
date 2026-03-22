using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Posts.SearchPosts;

public sealed class SearchPostsCommandValidator : AbstractValidator<SearchPostsCommand>
{
    public SearchPostsCommandValidator()
    {
        RuleFor(x => x.Filters)
            .Must(f =>
                (f == null) ||
                (f.Location == null && f.RadiusInKm == null) ||
                (f.Location != null && f.RadiusInKm != null))
            .WithMessage("Location and RadiusInKm must both be provided or both be omitted");

        When(x => x.Filters?.Location != null, () =>
        {
            RuleFor(x => x.Filters!.Location!.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Filters!.Location!.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Longitude must be between -180 and 180");
        });

        When(x => x.Filters?.RadiusInKm != null, () =>
        {
            RuleFor(x => x.Filters!.RadiusInKm!.Value)
                .InclusiveBetween(1, 20)
                .WithMessage("RadiusInKm must be between 1 and 20");
        });

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}
