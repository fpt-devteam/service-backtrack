    using FluentValidation;

    namespace Backtrack.Core.Application.Usecases.Organizations.CreateOrganization;

    public sealed class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
    {
        public CreateOrganizationCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(255).WithMessage("Name must not exceed 255 characters");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required")
                .MaximumLength(255).WithMessage("Slug must not exceed 255 characters")
                .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens");

            RuleFor(x => x.Location)
                .NotNull().WithMessage("Location is required");

            RuleFor(x => x.Location!.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
                .When(x => x.Location != null);

            RuleFor(x => x.Location!.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
                .When(x => x.Location != null);

            RuleFor(x => x.DisplayAddress)
                .NotEmpty().WithMessage("DisplayAddress is required")
                .MaximumLength(1000).WithMessage("DisplayAddress must not exceed 1000 characters");

            RuleFor(x => x.ExternalPlaceId)
                .MaximumLength(500).WithMessage("ExternalPlaceId must not exceed 500 characters");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .MaximumLength(50).WithMessage("Phone must not exceed 50 characters");

            RuleFor(x => x.IndustryType)
                .NotEmpty().WithMessage("Industry type is required")
                .MaximumLength(255).WithMessage("Industry type must not exceed 255 characters");

            RuleFor(x => x.TaxIdentificationNumber)
                .NotEmpty().WithMessage("Tax identification number is required")
                .MaximumLength(50).WithMessage("Tax identification number must not exceed 50 characters");

            RuleFor(x => x.LogoUrl)
                .NotEmpty().WithMessage("Logo URL is required")
                .MaximumLength(2048).WithMessage("Logo URL must not exceed 2048 characters");
        }
    }
