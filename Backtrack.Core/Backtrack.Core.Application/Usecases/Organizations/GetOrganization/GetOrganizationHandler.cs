using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts.CreatePost;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetOrganization;

public sealed class GetOrganizationHandler : IRequestHandler<GetOrganizationQuery, OrganizationResult>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;


    public GetOrganizationHandler(IOrganizationRepository organizationRepository, IMembershipRepository membershipRepository)
    {
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<OrganizationResult> Handle(GetOrganizationQuery query, CancellationToken cancellationToken)
    {
        var org = await _organizationRepository.GetByIdAsync(query.OrgId);
        if (org is null)
        {
            throw new NotFoundException(OrganizationErrors.NotFound);
        }
        if (string.IsNullOrEmpty(query.UserId))
        {
            throw new InvalidOperationException("UserId is not provided when initializing GetOrganizationQuery.");
        }
        var callerMembership = await _membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken);
        if (callerMembership is null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }

        return new OrganizationResult
        {
            Id = org.Id,
            Name = org.Name,
            Slug = org.Slug,
            Location = org.Location,
            DisplayAddress = org.DisplayAddress,
            ExternalPlaceId = org.ExternalPlaceId,
            Phone = org.Phone,
            IndustryType = org.IndustryType,
            TaxIdentificationNumber = org.TaxIdentificationNumber,
            Status = org.Status.ToString(),
            CreatedAt = org.CreatedAt,
        };
    }
}
