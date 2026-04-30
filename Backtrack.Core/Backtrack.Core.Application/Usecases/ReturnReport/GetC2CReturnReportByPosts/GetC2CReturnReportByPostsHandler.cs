using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetC2CReturnReportByPosts;

public sealed class GetC2CReturnReportByPostsHandler(
    IC2CReturnReportRepository returnReportRepository) : IRequestHandler<GetC2CReturnReportByPostsQuery, C2CReturnReportResult>
{
    public async Task<C2CReturnReportResult> Handle(GetC2CReturnReportByPostsQuery query, CancellationToken cancellationToken)
    {
        var returnReport = await returnReportRepository.GetByPostsAsync(query.FinderPostId, query.OwnerPostId, cancellationToken)
            ?? throw new NotFoundException(ReturnReportErrors.NotFound);

        if (returnReport.FinderId != query.UserId && returnReport.OwnerId != query.UserId)
        {
            throw new ForbiddenException(ReturnReportErrors.NotAuthorized);
        }

        return returnReport.ToC2CReturnReportResult();
    }
}
