using Backtrack.Core.Application.Posts.Commands.CreatePost;
using Backtrack.Core.Domain.Constants;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrack.Core.Application.Posts.Queries.GetPosts
{
    public sealed class GetPostsQueryValidator : AbstractValidator<GetPostsQuery>
    {
        public GetPostsQueryValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .When(x => x.Latitude.HasValue)
                .WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .When(x => x.Longitude.HasValue)
                .WithMessage("Longitude must be between -180 and 180");

            RuleFor(x => x.RadiusInKm)
                .GreaterThan(0)
                .LessThanOrEqualTo(10)
                .When(x => x.RadiusInKm.HasValue && x.Latitude.HasValue)
                .WithMessage("RadiusInKm must be greater than 0");

            RuleFor(x => x)
            .Must(x =>
                (x.Latitude.HasValue && x.Longitude.HasValue && x.RadiusInKm.HasValue) ||
                (!x.Latitude.HasValue && !x.Longitude.HasValue && !x.RadiusInKm.HasValue)
            )
            .WithMessage("Latitude, Longitude and RadiusInKm must be provided together");
        }
    }

}
