using FluentValidation;

namespace Backtrack.Core.Application.Usecases.ReturnReport.CreateOrgReturnReport;

public sealed class CreateOrgReturnReportCommandValidator : AbstractValidator<CreateOrgReturnReportCommand>
{
    public CreateOrgReturnReportCommandValidator()
    {
        RuleFor(x => x.EvidenceImageUrls)
            .NotEmpty().WithMessage("At least one evidence image URL is required.");
    }
}
