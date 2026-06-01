using FluentValidation;

namespace Backtrack.Core.Application.Usecases.QnA.CreateAnswer;

public sealed class CreateAnswerCommandValidator : AbstractValidator<CreateAnswerCommand>
{
    public CreateAnswerCommandValidator()
    {
        RuleFor(x => x.AnswerText)
            .NotEmpty().WithMessage("AnswerText is required.")
            .MaximumLength(2000).WithMessage("AnswerText must not exceed 2000 characters.")
            .When(x => x.Type == Domain.Constants.AnswerType.Text);

        RuleFor(x => x.ImageUrls)
            .NotEmpty().WithMessage("ImageUrls is required.")
            .When(x => x.Type == Domain.Constants.AnswerType.Image);
    }
}
