using FluentValidation;

namespace Backtrack.Core.Application.Usecases.ReturnReport.InitiateC2CReturnReport;

public sealed class InitiateC2CReturnReportCommandValidator : AbstractValidator<InitiateC2CReturnReportCommand>
{
    public InitiateC2CReturnReportCommandValidator()
    {
        RuleFor(x => x.FinderPostId)
            .NotEmpty().WithMessage("FinderPostId is required.");

        RuleFor(x => x.OwnerPostId)
            .NotEmpty().WithMessage("OwnerPostId is required.")
            .NotEqual(x => x.FinderPostId).WithMessage("FinderPostId and OwnerPostId must be different.");
    }
}
