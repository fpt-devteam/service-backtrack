using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Users;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetC2CReturnReportById;

public sealed class GetC2CReturnReportByIdHandler(
    IC2CReturnReportRepository returnReportRepository) : IRequestHandler<GetC2CReturnReportByIdQuery, C2CReturnReportResult>
{
    public async Task<C2CReturnReportResult> Handle(GetC2CReturnReportByIdQuery query, CancellationToken cancellationToken)
    {
        var returnReport = await returnReportRepository.GetByIdWithExtensionAsync(query.C2CReturnReportId, cancellationToken)
            ?? throw new NotFoundException(ReturnReportErrors.NotFound);

        if (returnReport.FinderId != query.UserId && returnReport.OwnerId != query.UserId)
        {
            throw new ForbiddenException(new Error("NotAuthorized", "You are not authorized to view this return report."));
        }

        return new C2CReturnReportResult
        {
            Id = returnReport.Id,
            Finder = returnReport.Finder.ToUserResult(),
            Owner = returnReport.Owner.ToUserResult(),
            Status = returnReport.Status.ToString(),
            ConfirmedAt = returnReport.ConfirmedAt,
            ExpiresAt = returnReport.ExpiresAt,
            CreatedAt = returnReport.CreatedAt
        };
    }
}
