using FluentValidation;

namespace Backtrack.Core.Application.Users.Commands.EnsureUserExist;

public sealed class EnsureUserExistCommandValidator : AbstractValidator<EnsureUserExistCommand>
{
    public EnsureUserExistCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Email must be a valid email address");
        });

        // RuleFor(x => x)
        //     .Must(x => !string.IsNullOrWhiteSpace(x.Email) || x.DisplayName != null)
        //     .WithMessage("At least one field (Email or DisplayName) must be provided for upsert");
    }
}
