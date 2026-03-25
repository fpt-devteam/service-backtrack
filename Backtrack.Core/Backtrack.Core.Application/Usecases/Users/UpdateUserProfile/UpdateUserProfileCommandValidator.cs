using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Users.UpdateUserProfile;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("DisplayName must not be empty")
            .MaximumLength(255).WithMessage("DisplayName must not exceed 255 characters")
            .When(x => x.DisplayName != null);

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("AvatarUrl must not exceed 500 characters")
            .When(x => x.AvatarUrl != null);

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters")
            .When(x => x.Phone != null);
    }
}
