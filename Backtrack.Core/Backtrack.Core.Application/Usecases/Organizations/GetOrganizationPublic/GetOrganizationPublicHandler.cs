using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetOrganizationPublic;

public sealed class GetOrganizationPublicHandler : IRequestHandler<GetOrganizationPublicQuery, OrganizationResult>
{
    private readonly IOrganizationRepository _organizationRepository;

    public GetOrganizationPublicHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<OrganizationResult> Handle(GetOrganizationPublicQuery query, CancellationToken cancellationToken)
    {
        var org = await _organizationRepository.GetByIdAsync(query.OrgId);
        if (org is null)
        {
            throw new NotFoundException(OrganizationErrors.NotFound);
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
            LogoUrl = org.LogoUrl,
            Status = org.Status.ToString(),
            CreatedAt = org.CreatedAt,
        };
    }
}
