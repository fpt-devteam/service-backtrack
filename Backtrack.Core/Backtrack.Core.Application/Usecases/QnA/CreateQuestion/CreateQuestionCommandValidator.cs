using FluentValidation;

namespace Backtrack.Core.Application.Usecases.QnA.CreateQuestion;

public sealed class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("QuestionText is required.")
            .MaximumLength(1000).WithMessage("QuestionText must not exceed 1000 characters.");
    }
}
