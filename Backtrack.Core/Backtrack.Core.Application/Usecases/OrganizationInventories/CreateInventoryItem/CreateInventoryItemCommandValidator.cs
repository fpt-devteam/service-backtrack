using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Organizations.CreateInventoryItem;

public sealed class CreateInventoryItemCommandValidator : AbstractValidator<CreateInventoryItemCommand>
{
    public CreateInventoryItemCommandValidator()
    {
        RuleFor(x => x.OrgId)
            .NotEmpty().WithMessage("Organization ID is required");

        RuleFor(x => x.ItemName)
            .NotEmpty().WithMessage("ItemName is required")
            .MaximumLength(500).WithMessage("ItemName must not exceed 500 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.DistinctiveMarks)
            .MaximumLength(500).WithMessage("DistinctiveMarks must not exceed 500 characters");

        RuleFor(x => x.StorageLocation)
            .MaximumLength(500).WithMessage("StorageLocation must not exceed 500 characters");

        RuleFor(x => x.FinderContact)
            .NotNull().WithMessage("FinderContact is required");

        RuleFor(x => x.FinderContact.Name)
            .NotEmpty().WithMessage("FinderContact.Name is required")
            .MaximumLength(255).WithMessage("FinderContact.Name must not exceed 255 characters");

        RuleFor(x => x.FinderContact.Email)
            .MaximumLength(255).WithMessage("FinderContact.Email must not exceed 255 characters")
            .EmailAddress().WithMessage("FinderContact.Email must be a valid email address")
            .When(x => x.FinderContact?.Email != null);

        RuleFor(x => x.FinderContact.Phone)
            .MaximumLength(50).WithMessage("FinderContact.Phone must not exceed 50 characters")
            .Matches(@"^\+?[0-9\s\-\(\)]{7,50}$").WithMessage("FinderContact.Phone must be a valid phone number")
            .When(x => x.FinderContact?.Phone != null);

        RuleFor(x => x.FinderContact.NationalId)
            .MaximumLength(50).WithMessage("FinderContact.NationalId must not exceed 50 characters")
            .When(x => x.FinderContact?.NationalId != null);

        RuleFor(x => x.FinderContact.OrgMemberId)
            .MaximumLength(100).WithMessage("FinderContact.OrgMemberId must not exceed 100 characters")
            .When(x => x.FinderContact?.OrgMemberId != null);

    }
}
