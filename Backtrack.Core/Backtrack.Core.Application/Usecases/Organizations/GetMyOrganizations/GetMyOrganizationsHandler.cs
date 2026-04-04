using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetMyOrganizations;

public sealed class GetMyOrganizationsHandler : IRequestHandler<GetMyOrganizationsQuery, List<MyOrganizationResult>>
{
    private readonly IMembershipRepository _membershipRepository;

    public GetMyOrganizationsHandler(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<List<MyOrganizationResult>> Handle(GetMyOrganizationsQuery query, CancellationToken cancellationToken)
    {
        var memberships = await _membershipRepository.GetByUserAsync(query.UserId, cancellationToken);

        return memberships.Select(m => new MyOrganizationResult
        {
            OrgId = m.OrganizationId,
            Name = m.Organization.Name,
            Slug = m.Organization.Slug,
            Location = m.Organization.Location,
            DisplayAddress = m.Organization.DisplayAddress,
            ExternalPlaceId = m.Organization.ExternalPlaceId,
            Phone = m.Organization.Phone,
            ContactEmail = m.Organization.ContactEmail,
            IndustryType = m.Organization.IndustryType,
            TaxIdentificationNumber = m.Organization.TaxIdentificationNumber,
            LogoUrl = m.Organization.LogoUrl,
            CoverImageUrl = m.Organization.CoverImageUrl,
            LocationNote = m.Organization.LocationNote,
            BusinessHours = m.Organization.BusinessHours,
            RequiredFinderContractFields = m.Organization.RequiredFinderContractFields,
            RequiredOwnerContractFields = m.Organization.RequiredOwnerContractFields,
            OrgStatus = m.Organization.Status.ToString(),
            MyRole = m.Role.ToString(),
            JoinedAt = m.JoinedAt,
        }).ToList();
    }
}
