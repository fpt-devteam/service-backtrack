using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.CreateOrganization;

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

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters")
            .When(x => x.Address is not null);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .MaximumLength(50).WithMessage("Phone must not exceed 50 characters");

        RuleFor(x => x.IndustryType)
            .NotEmpty().WithMessage("Industry type is required")
            .MaximumLength(255).WithMessage("Industry type must not exceed 255 characters");

        RuleFor(x => x.TaxIdentificationNumber)
            .NotEmpty().WithMessage("Tax identification number is required")
            .MaximumLength(50).WithMessage("Tax identification number must not exceed 50 characters");
    }
}
