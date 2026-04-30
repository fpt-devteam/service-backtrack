using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetOrgReturnReports;

public sealed class GetOrgReturnReportsHandler(
    IOrgReturnReportRepository orgReturnReportRepository,
    IMembershipRepository membershipRepository) : IRequestHandler<GetOrgReturnReportsQuery, PagedResult<OrgReturnReportResult>>
{
    public async Task<PagedResult<OrgReturnReportResult>> Handle(GetOrgReturnReportsQuery query, CancellationToken cancellationToken)
    {
        var membership = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        // Admins see all reports; staff see only their own
        var staffIdFilter = membership.Role == MembershipRole.OrgAdmin ? null : query.UserId;

        var (items, total) = await orgReturnReportRepository.GetByOrgAsync(
            query.OrgId, staffIdFilter, query.Page, query.PageSize, cancellationToken);

        var results = items.ConvertAll(r => r.ToOrgReturnReportResult());

        return new PagedResult<OrgReturnReportResult>(total, results);
    }
}
