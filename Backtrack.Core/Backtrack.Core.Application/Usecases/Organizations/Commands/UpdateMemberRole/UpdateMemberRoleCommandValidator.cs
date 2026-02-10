using FluentValidation;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.UpdateMemberRole;

public sealed class UpdateMemberRoleCommandValidator : AbstractValidator<UpdateMemberRoleCommand>
{
    public UpdateMemberRoleCommandValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required");
    }
}
