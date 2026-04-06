using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetOrgReturnReports;

public sealed class GetOrgReturnReportsHandler(
    IOrgReturnReportRepository orgReturnReportRepository,
    IMembershipRepository membershipRepository,
    IOrganizationRepository organizationRepository) : IRequestHandler<GetOrgReturnReportsQuery, PagedResult<OrgReturnReportResult>>
{
    public async Task<PagedResult<OrgReturnReportResult>> Handle(GetOrgReturnReportsQuery query, CancellationToken cancellationToken)
    {
        var membership = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        var org = await organizationRepository.GetByIdAsync(query.OrgId)
            ?? throw new NotFoundException(OrganizationErrors.NotFound);

        // Admins see all reports; staff see only their own
        var staffIdFilter = membership.Role == MembershipRole.OrgAdmin ? null : query.UserId;

        var (items, total) = await orgReturnReportRepository.GetByOrgAsync(
            query.OrgId, staffIdFilter, query.Page, query.PageSize, cancellationToken);

        var orgResult = new OrganizationResult
        {
            Id = org.Id,
            Name = org.Name,
            Slug = org.Slug,
            Location = org.Location,
            DisplayAddress = org.DisplayAddress,
            ExternalPlaceId = org.ExternalPlaceId,
            Phone = org.Phone,
            ContactEmail = org.ContactEmail,
            IndustryType = org.IndustryType,
            TaxIdentificationNumber = org.TaxIdentificationNumber,
            LogoUrl = org.LogoUrl,
            CoverImageUrl = org.CoverImageUrl,
            LocationNote = org.LocationNote,
            BusinessHours = org.BusinessHours,
            RequiredFinderContractFields = org.RequiredFinderContractFields,
            RequiredOwnerContractFields = org.RequiredOwnerContractFields,
            Status = org.Status.ToString(),
            CreatedAt = org.CreatedAt,
        };

        var results = items.Select(r => new OrgReturnReportResult
        {
            Id = r.Id,
            Organization = orgResult,
            Staff = r.Staff!.ToUserResult(),
            OwnerInfo = r.OwnerInfo,
            Post = r.Post?.ToPostResult(),
            ExpiresAt = r.ExpiresAt,
            CreatedAt = r.CreatedAt
        }).ToList();

        return new PagedResult<OrgReturnReportResult>(total, results);
    }
}
