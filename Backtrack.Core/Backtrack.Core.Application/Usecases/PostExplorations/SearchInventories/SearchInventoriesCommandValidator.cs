using FluentValidation;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SearchInventories;

public sealed class SearchInventoriesCommandValidator : AbstractValidator<SearchInventoriesCommand>
{
    public SearchInventoriesCommandValidator()
    {
        RuleFor(x => x.Query)
            .MaximumLength(500).WithMessage("Query must not exceed 500 characters")
            .When(x => x.Query != null);
    }
}
