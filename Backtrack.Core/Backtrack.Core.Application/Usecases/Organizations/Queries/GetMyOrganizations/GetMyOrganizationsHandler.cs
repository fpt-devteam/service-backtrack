using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Queries.GetMyOrganizations;

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
            Address = m.Organization.Address,
            Phone = m.Organization.Phone,
            IndustryType = m.Organization.IndustryType,
            TaxIdentificationNumber = m.Organization.TaxIdentificationNumber,
            OrgStatus = m.Organization.Status.ToString(),
            MyRole = m.Role.ToString(),
            JoinedAt = m.JoinedAt,
        }).ToList();
    }
}
