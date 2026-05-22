using FluentValidation;

namespace Backtrack.Core.Application.Usecases.QnA.AnswerQuestion;

public sealed class AnswerQuestionCommandValidator : AbstractValidator<AnswerQuestionCommand>
{
    public AnswerQuestionCommandValidator()
    {
        RuleFor(x => x.AnswerText)
            .NotEmpty().WithMessage("AnswerText is required.")
            .MaximumLength(2000).WithMessage("AnswerText must not exceed 2000 characters.");
    }
}
