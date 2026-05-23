using FluentValidation;

namespace Backtrack.Core.Application.Usecases.QnA.CreateQuestions;

public sealed class CreateQuestionsCommandValidator : AbstractValidator<CreateQuestionsCommand>
{
    public CreateQuestionsCommandValidator()
    {
        RuleFor(x => x.QuestionTexts)
            .NotEmpty().WithMessage("At least one question is required.")
            .Must(q => q.Count <= 20).WithMessage("Cannot create more than 20 questions at once.");

        RuleForEach(x => x.QuestionTexts)
            .NotEmpty().WithMessage("Question text cannot be empty.")
            .MaximumLength(1000).WithMessage("Question text must not exceed 1000 characters.");
    }
}
