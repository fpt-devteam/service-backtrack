using FluentValidation;

namespace Backtrack.Core.Application.Usecases.PostExplorations.SemanticSearchPost;

public sealed class SemanticSearchPostCommandValidator : AbstractValidator<SemanticSearchPostCommand>
{
    public SemanticSearchPostCommandValidator()
    {
        When(x => x.Filters?.Time?.From != null && x.Filters?.Time?.To != null, () =>
        {
            RuleFor(x => x.Filters!.Time!.To!.Value)
                .GreaterThanOrEqualTo(x => x.Filters!.Time!.From!.Value)
                .WithMessage("Time.To must be greater than or equal to Time.From");
        });
    }
}
