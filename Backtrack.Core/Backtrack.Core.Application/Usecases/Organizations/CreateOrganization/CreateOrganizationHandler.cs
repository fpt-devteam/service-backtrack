using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.CreateOrganization;

public sealed class CreateOrganizationHandler : IRequestHandler<CreateOrganizationCommand, OrganizationResult>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IEventPublisher _eventPublisher;

    public CreateOrganizationHandler(
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository,
        IEventPublisher eventPublisher)
    {
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<OrganizationResult> Handle(CreateOrganizationCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.UserId))
        {
            throw new InvalidOperationException("UserId is not provided when initializing CreateOrganizationCommand.");
        }

        if (await _organizationRepository.SlugExistsAsync(command.Slug, cancellationToken))
            throw new ConflictException(OrganizationErrors.SlugAlreadyExists);

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Slug = command.Slug,
            Location = command.Location,
            DisplayAddress = command.DisplayAddress,
            ExternalPlaceId = command.ExternalPlaceId,
            Phone = command.Phone,
            ContactEmail = command.ContactEmail,
            IndustryType = command.IndustryType,
            TaxIdentificationNumber = command.TaxIdentificationNumber,
            LogoUrl = command.LogoUrl,
            CoverImageUrl = command.CoverImageUrl,
            LocationNote = command.LocationNote,
            BusinessHours = command.BusinessHours,
            RequiredFinderContractFields = command.RequiredFinderContractFields,
            RequiredOwnerContractFields = command.RequiredOwnerContractFields,
            Status = OrganizationStatus.Active,
        };

        await _organizationRepository.CreateAsync(organization);

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            UserId = command.UserId,
            Role = MembershipRole.OrgAdmin,
            Status = MembershipStatus.Active,
            JoinedAt = DateTimeOffset.UtcNow,
        };

        await _membershipRepository.CreateAsync(membership);
        await _organizationRepository.SaveChangesAsync();

        await _eventPublisher.PublishOrgEnsureExistAsync(new OrgEnsureExistIntegrationEvent
        {
            Id = organization.Id.ToString(),
            Name = organization.Name,
            Slug = organization.Slug,
            LogoUrl = organization.LogoUrl,
            EventTimestamp = DateTimeOffset.UtcNow,
        });

        return new OrganizationResult
        {
            Id = organization.Id,
            Name = organization.Name,
            Slug = organization.Slug,
            Location = organization.Location,
            DisplayAddress = organization.DisplayAddress,
            ExternalPlaceId = organization.ExternalPlaceId,
            Phone = organization.Phone,
            ContactEmail = organization.ContactEmail,
            IndustryType = organization.IndustryType,
            TaxIdentificationNumber = organization.TaxIdentificationNumber,
            LogoUrl = organization.LogoUrl,
            CoverImageUrl = organization.CoverImageUrl,
            LocationNote = organization.LocationNote,
            BusinessHours = organization.BusinessHours,
            RequiredFinderContractFields = organization.RequiredFinderContractFields,
            RequiredOwnerContractFields = organization.RequiredOwnerContractFields,
            Status = organization.Status.ToString(),
            CreatedAt = organization.CreatedAt,
        };
    }
}
