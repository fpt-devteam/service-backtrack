using FluentValidation;

namespace Backtrack.Core.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(x => x.DisplayName)
            .MaximumLength(100).WithMessage("DisplayName must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));
    }
}