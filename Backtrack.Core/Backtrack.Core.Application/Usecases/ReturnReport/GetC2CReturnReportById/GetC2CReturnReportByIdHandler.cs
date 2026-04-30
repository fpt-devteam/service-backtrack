using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetC2CReturnReportById;

public sealed class GetC2CReturnReportByIdHandler(
    IC2CReturnReportRepository returnReportRepository) : IRequestHandler<GetC2CReturnReportByIdQuery, C2CReturnReportResult>
{
    public async Task<C2CReturnReportResult> Handle(GetC2CReturnReportByIdQuery query, CancellationToken cancellationToken)
    {
        var returnReport = await returnReportRepository.GetByIdWithPostsAsync(query.C2CReturnReportId, cancellationToken)
            ?? throw new NotFoundException(ReturnReportErrors.NotFound);

        if (returnReport.FinderId != query.UserId && returnReport.OwnerId != query.UserId)
        {
            throw new ForbiddenException(ReturnReportErrors.NotAuthorized);
        }

        return returnReport.ToC2CReturnReportResult();
    }
}
