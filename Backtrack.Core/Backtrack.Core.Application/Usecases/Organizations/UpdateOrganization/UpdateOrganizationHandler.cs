using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.UpdateOrganization;

public sealed class UpdateOrganizationHandler : IRequestHandler<UpdateOrganizationCommand, OrganizationResult>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IEventPublisher _eventPublisher;

    public UpdateOrganizationHandler(
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository,
        IEventPublisher eventPublisher)
    {
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
        _eventPublisher = eventPublisher;
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

        if (command.Slug != null && org.Slug != command.Slug)
        {
            var slugExists = await _organizationRepository.SlugExistsAsync(command.Slug, cancellationToken);
            if (slugExists)
            {
                throw new ConflictException(OrganizationErrors.SlugAlreadyExists);
            }
        }

        org.Name = command.Name ?? org.Name;
        org.Slug = command.Slug ?? org.Slug;
        if (command.Location != null)
        {
            org.Location = new GeoPoint(command.Location.Latitude, command.Location.Longitude);
        }
        org.DisplayAddress = command.DisplayAddress ?? org.DisplayAddress;
        org.ExternalPlaceId = command.ExternalPlaceId ?? org.ExternalPlaceId;
        org.Phone = command.Phone ?? org.Phone;
        org.ContactEmail = command.ContactEmail ?? org.ContactEmail;
        org.IndustryType = command.IndustryType ?? org.IndustryType;
        org.TaxIdentificationNumber = command.TaxIdentificationNumber ?? org.TaxIdentificationNumber;
        org.LogoUrl = command.LogoUrl ?? org.LogoUrl;
        org.CoverImageUrl = command.CoverImageUrl ?? org.CoverImageUrl;
        org.LocationNote = command.LocationNote ?? org.LocationNote;
        org.BusinessHours = command.BusinessHours ?? org.BusinessHours;
        org.RequiredFinderContractFields = command.RequiredFinderContractFields ?? org.RequiredFinderContractFields;
        org.RequiredOwnerContractFields = command.RequiredOwnerContractFields ?? org.RequiredOwnerContractFields;

        _organizationRepository.Update(org);
        await _organizationRepository.SaveChangesAsync();

        await _eventPublisher.PublishOrgEnsureExistAsync(new OrgEnsureExistIntegrationEvent
        {
            Id = org.Id.ToString(),
            Name = org.Name,
            Slug = org.Slug,
            LogoUrl = org.LogoUrl,
            EventTimestamp = DateTimeOffset.UtcNow,
        });

        return new OrganizationResult
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
    }
}
