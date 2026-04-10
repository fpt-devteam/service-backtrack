using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetOrganizationById;

public sealed class GetOrganizationByIdHandler(
    IOrganizationRepository organizationRepository,
    IMembershipRepository membershipRepository)
    : IRequestHandler<GetOrganizationByIdQuery, OrganizationResult>
{
    public async Task<OrganizationResult> Handle(GetOrganizationByIdQuery query, CancellationToken cancellationToken)
    {
        var org = await organizationRepository.GetByIdAsync(query.OrgId)
            ?? throw new NotFoundException(OrganizationErrors.NotFound);

        if (string.IsNullOrEmpty(query.UserId))
            throw new InvalidOperationException("UserId is not provided when initializing GetOrganizationByIdQuery.");

        var callerMembership = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken);
        if (callerMembership is null)
            throw new ForbiddenException(MembershipErrors.NotAMember);

        return new OrganizationResult
        {
            Id                           = org.Id,
            Name                         = org.Name,
            Slug                         = org.Slug,
            Location                     = org.Location,
            DisplayAddress               = org.DisplayAddress,
            ExternalPlaceId              = org.ExternalPlaceId,
            Phone                        = org.Phone,
            ContactEmail                 = org.ContactEmail,
            IndustryType                 = org.IndustryType,
            TaxIdentificationNumber      = org.TaxIdentificationNumber,
            LogoUrl                      = org.LogoUrl,
            CoverImageUrl                = org.CoverImageUrl,
            LocationNote                 = org.LocationNote,
            BusinessHours                = org.BusinessHours,
            RequiredFinderContractFields = org.RequiredFinderContractFields,
            RequiredOwnerContractFields  = org.RequiredOwnerContractFields,
            Status                       = org.Status.ToString(),
            CreatedAt                    = org.CreatedAt,
        };
    }
}
