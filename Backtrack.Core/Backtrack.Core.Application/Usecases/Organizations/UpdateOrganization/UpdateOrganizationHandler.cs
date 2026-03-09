using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts.CreatePost;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.UpdateOrganization;

public sealed class UpdateOrganizationHandler : IRequestHandler<UpdateOrganizationCommand, OrganizationResult>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;

    public UpdateOrganizationHandler(
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository)
    {
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<OrganizationResult> Handle(UpdateOrganizationCommand command, CancellationToken cancellationToken)
    {
        var callerMembership = await _membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken);
        if (callerMembership is null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }
        if (callerMembership.Role != MembershipRole.OrgAdmin)
        {
            throw new ForbiddenException(MembershipErrors.InsufficientRole);
        }

        var org = await _organizationRepository.GetByIdAsync(command.OrgId, isTrack: true);
        if (org is null)
        {
            throw new NotFoundException(OrganizationErrors.NotFound);
        }

        if (org.Slug != command.Slug)
        {
            var slugExists = await _organizationRepository.SlugExistsAsync(command.Slug, cancellationToken);
            if (slugExists)
            {
                throw new ConflictException(OrganizationErrors.SlugAlreadyExists);
            }
        }

        org.Name = command.Name;
        org.Slug = command.Slug;
        if (command.Location != null)
        {
            org.Location = new GeoPoint(command.Location.Latitude, command.Location.Longitude);
        }
        org.DisplayAddress = command.DisplayAddress ?? org.DisplayAddress;
        org.ExternalPlaceId = command.ExternalPlaceId ?? org.ExternalPlaceId;
        org.Phone = command.Phone;
        org.IndustryType = command.IndustryType;
        org.TaxIdentificationNumber = command.TaxIdentificationNumber;

        _organizationRepository.Update(org);
        await _organizationRepository.SaveChangesAsync();

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
