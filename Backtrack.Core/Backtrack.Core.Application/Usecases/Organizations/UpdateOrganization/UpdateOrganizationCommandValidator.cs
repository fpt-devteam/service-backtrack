using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Organizations.UpdateOrganization;

public sealed class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name must not be empty")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug must not be empty")
            .MaximumLength(255).WithMessage("Slug must not exceed 255 characters")
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens")
            .When(x => x.Slug != null);

        RuleFor(x => x.Location!.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.Location != null);

        RuleFor(x => x.Location!.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.Location != null);

        RuleFor(x => x.DisplayAddress)
            .NotEmpty().WithMessage("DisplayAddress must not be empty")
            .MaximumLength(1000).WithMessage("DisplayAddress must not exceed 1000 characters")
            .When(x => x.DisplayAddress != null);

        RuleFor(x => x.ExternalPlaceId)
            .MaximumLength(500).WithMessage("ExternalPlaceId must not exceed 500 characters")
            .When(x => x.ExternalPlaceId != null);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone must not be empty")
            .MaximumLength(50).WithMessage("Phone must not exceed 50 characters")
            .When(x => x.Phone != null);

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Contact email must be a valid email address")
            .MaximumLength(255).WithMessage("Contact email must not exceed 255 characters")
            .When(x => x.ContactEmail != null);

        RuleFor(x => x.IndustryType)
            .NotEmpty().WithMessage("Industry type must not be empty")
            .MaximumLength(255).WithMessage("Industry type must not exceed 255 characters")
            .When(x => x.IndustryType != null);

        RuleFor(x => x.TaxIdentificationNumber)
            .NotEmpty().WithMessage("Tax identification number must not be empty")
            .MaximumLength(50).WithMessage("Tax identification number must not exceed 50 characters")
            .When(x => x.TaxIdentificationNumber != null);

        RuleFor(x => x.LogoUrl)
            .NotEmpty().WithMessage("Logo URL must not be empty")
            .MaximumLength(2048).WithMessage("Logo URL must not exceed 2048 characters")
            .When(x => x.LogoUrl != null);

        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(2048).WithMessage("Cover image URL must not exceed 2048 characters")
            .When(x => x.CoverImageUrl != null);

        RuleFor(x => x.LocationNote)
            .MaximumLength(1000).WithMessage("Location note must not exceed 1000 characters")
            .When(x => x.LocationNote != null);

        RuleForEach(x => x.BusinessHours)
            .ChildRules(day =>
            {
                day.RuleFor(d => d.Day)
                    .IsInEnum().WithMessage("Day must be a valid day of the week");

                day.RuleFor(d => d.OpenTime)
                    .Matches(@"^\d{2}:\d{2}$").WithMessage("OpenTime must be in HH:mm format")
                    .When(d => !d.IsClosed && !string.IsNullOrEmpty(d.OpenTime));

                day.RuleFor(d => d.CloseTime)
                    .Matches(@"^\d{2}:\d{2}$").WithMessage("CloseTime must be in HH:mm format")
                    .When(d => !d.IsClosed && !string.IsNullOrEmpty(d.CloseTime));
            })
            .When(x => x.BusinessHours != null);
    }
}
