    using Backtrack.Core.Domain.Constants;
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

            RuleFor(x => x.ContactEmail)
                .EmailAddress().WithMessage("Contact email must be a valid email address")
                .MaximumLength(255).WithMessage("Contact email must not exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));

            RuleFor(x => x.CoverImageUrl)
                .MaximumLength(2048).WithMessage("Cover image URL must not exceed 2048 characters")
                .When(x => !string.IsNullOrEmpty(x.CoverImageUrl));

            RuleFor(x => x.LocationNote)
                .MaximumLength(1000).WithMessage("Location note must not exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.LocationNote));

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

            RuleFor(x => x.RequiredFinderContactFields)
                .NotEmpty().WithMessage("RequiredFinderContactFields must contain at least one field");

            RuleForEach(x => x.RequiredFinderContactFields)
                .IsInEnum().WithMessage("Each entry in RequiredFinderContactFields must be a valid FinderContactField")
                .When(x => x.RequiredFinderContactFields != null);

            When(x => x.RequiredOwnerFormFields != null, () =>
            {
                RuleForEach(x => x.RequiredOwnerFormFields!).ChildRules(field =>
                {
                    field.RuleFor(f => f.Key)
                        .NotEmpty().WithMessage("Owner form field key is required");

                    field.RuleFor(f => f.Label)
                        .NotEmpty().WithMessage("Owner form field label is required");

                    field.RuleFor(f => f.Type)
                        .IsInEnum().WithMessage("Owner form field type is invalid");
                });
            });
        }
    }
