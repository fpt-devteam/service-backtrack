using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Commands.CreateOrganization;

public sealed class CreateOrganizationHandler : IRequestHandler<CreateOrganizationCommand, OrganizationResult>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;

    public CreateOrganizationHandler(
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository)
    {
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<OrganizationResult> Handle(CreateOrganizationCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.UserId))
        {
            throw new InvalidOperationException("UserId is not provided when initializing CreateOrganizationCommand.");
        }
        var slugExists = await _organizationRepository.SlugExistsAsync(command.Slug, cancellationToken);
        if (slugExists)
        {
            throw new ConflictException(OrganizationErrors.SlugAlreadyExists);
        }

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Slug = command.Slug,
            Address = command.Address,
            Phone = command.Phone,
            IndustryType = command.IndustryType,
            TaxIdentificationNumber = command.TaxIdentificationNumber,
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

        return new OrganizationResult
        {
            Id = organization.Id,
            Name = organization.Name,
            Slug = organization.Slug,
            Address = organization.Address,
            Phone = organization.Phone,
            IndustryType = organization.IndustryType,
            TaxIdentificationNumber = organization.TaxIdentificationNumber,
            Status = organization.Status.ToString(),
            CreatedAt = organization.CreatedAt,
        };
    }
}
