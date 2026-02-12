using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.CreateInvitation;

public sealed class CreateInvitationCommandValidator : AbstractValidator<CreateInvitationCommand>
{
    public CreateInvitationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required");

        RuleFor(x => x.OrgId)
            .NotEmpty().WithMessage("Organization ID is required");
    }
}
