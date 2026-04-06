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
        var membership = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        var returnReport = await orgReturnReportRepository.GetByIdWithDetailsAsync(query.ReturnReportId, cancellationToken)
            ?? throw new NotFoundException(ReturnReportErrors.NotFound);

        if (returnReport.OrgId != query.OrgId)
            throw new ForbiddenException(new Error("NotInOrg", "This return report does not belong to your organization."));

        return new OrgReturnReportResult
        {
            Id = returnReport.Id,
            Organization = new OrganizationResult
            {
                Id = returnReport.Organization!.Id,
                Name = returnReport.Organization.Name,
                Slug = returnReport.Organization.Slug,
                Location = returnReport.Organization.Location,
                DisplayAddress = returnReport.Organization.DisplayAddress,
                ExternalPlaceId = returnReport.Organization.ExternalPlaceId,
                Phone = returnReport.Organization.Phone,
                ContactEmail = returnReport.Organization.ContactEmail,
                IndustryType = returnReport.Organization.IndustryType,
                TaxIdentificationNumber = returnReport.Organization.TaxIdentificationNumber,
                LogoUrl = returnReport.Organization.LogoUrl,
                CoverImageUrl = returnReport.Organization.CoverImageUrl,
                LocationNote = returnReport.Organization.LocationNote,
                BusinessHours = returnReport.Organization.BusinessHours,
                RequiredFinderContractFields = returnReport.Organization.RequiredFinderContractFields,
                RequiredOwnerContractFields = returnReport.Organization.RequiredOwnerContractFields,
                Status = returnReport.Organization.Status.ToString(),
                CreatedAt = returnReport.Organization.CreatedAt,
            },
            Staff = returnReport.Staff!.ToUserResult(),
            OwnerInfo = returnReport.OwnerInfo,
            Post = returnReport.Post?.ToPostResult(),
            ExpiresAt = returnReport.ExpiresAt,
            CreatedAt = returnReport.CreatedAt
        };
    }
}
