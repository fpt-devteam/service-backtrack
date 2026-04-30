using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetOrgReturnReportById;

public sealed class GetOrgReturnReportByIdHandler(
    IOrgReturnReportRepository orgReturnReportRepository,
    IMembershipRepository membershipRepository) : IRequestHandler<GetOrgReturnReportByIdQuery, OrgReturnReportResult>
{
    public async Task<OrgReturnReportResult> Handle(GetOrgReturnReportByIdQuery query, CancellationToken cancellationToken)
    {
        if (await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken) == null)
            throw new ForbiddenException(MembershipErrors.NotAMember);

        var returnReport = await orgReturnReportRepository.GetByIdWithDetailsAsync(query.ReturnReportId, cancellationToken)
            ?? throw new NotFoundException(ReturnReportErrors.NotFound);

        if (returnReport.OrgId != query.OrgId)
            throw new ForbiddenException(ReturnReportErrors.PostNotInOrg);

        return returnReport.ToOrgReturnReportResult();
    }
}
