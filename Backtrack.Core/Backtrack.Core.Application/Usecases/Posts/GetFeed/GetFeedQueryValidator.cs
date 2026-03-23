using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Posts.GetFeed;

public sealed class GetFeedQueryValidator : AbstractValidator<GetFeedQuery>
{
    public GetFeedQueryValidator()
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
    }
}
