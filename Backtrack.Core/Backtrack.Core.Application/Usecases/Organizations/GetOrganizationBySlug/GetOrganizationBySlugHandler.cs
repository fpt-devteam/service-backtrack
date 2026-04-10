using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetOrganizationBySlug;

public sealed class GetOrganizationBySlugHandler(IOrganizationRepository organizationRepository)
    : IRequestHandler<GetOrganizationBySlugQuery, OrganizationResult>
{
    public async Task<OrganizationResult> Handle(GetOrganizationBySlugQuery query, CancellationToken cancellationToken)
    {
        var org = await organizationRepository.GetBySlugAsync(query.Slug, cancellationToken)
            ?? throw new NotFoundException(OrganizationErrors.NotFound);

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
