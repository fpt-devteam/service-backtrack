using FluentValidation;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SearchInventories;

public sealed class SearchInventoriesCommandValidator : AbstractValidator<SearchInventoriesCommand>
{
    public SearchInventoriesCommandValidator()
    {
        RuleFor(x => x.Query)
            .MaximumLength(500).WithMessage("Query must not exceed 500 characters")
            .When(x => x.Query != null);

        When(x => x.Filters?.Time?.From != null && x.Filters?.Time?.To != null, () =>
        {
            RuleFor(x => x.Filters!.Time!.To!.Value)
                .GreaterThanOrEqualTo(x => x.Filters!.Time!.From!.Value)
                .WithMessage("Time.To must be greater than or equal to Time.From");
        });
    }
}
