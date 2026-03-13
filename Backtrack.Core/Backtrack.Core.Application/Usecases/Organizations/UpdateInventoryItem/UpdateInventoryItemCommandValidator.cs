using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Organizations.UpdateInventoryItem;

public sealed class UpdateInventoryItemCommandValidator : AbstractValidator<UpdateInventoryItemCommand>
{
    public UpdateInventoryItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required");

        RuleFor(x => x.OrgId)
            .NotEmpty().WithMessage("Organization ID is required");

        RuleFor(x => x.ItemName)
            .MaximumLength(500).WithMessage("ItemName must not exceed 500 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.DistinctiveMarks)
            .MaximumLength(500).WithMessage("DistinctiveMarks must not exceed 500 characters");

        RuleFor(x => x.StorageLocation)
            .MaximumLength(500).WithMessage("StorageLocation must not exceed 500 characters");
    }
}
