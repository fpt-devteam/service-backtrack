using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetAllOrganizations;

public sealed class GetAllOrganizationsHandler
    : IRequestHandler<GetAllOrganizationsQuery, (List<OrganizationResult> Items, int Total)>
{
    private readonly IOrganizationRepository _organizationRepository;

    public GetAllOrganizationsHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<(List<OrganizationResult> Items, int Total)> Handle(
        GetAllOrganizationsQuery query, CancellationToken cancellationToken)
    {
        var (orgs, total) = await _organizationRepository.GetAllAsync(query.Page, query.PageSize, cancellationToken);

        var items = orgs.Select(o => new OrganizationResult
        {
            Id = o.Id,
            Name = o.Name,
            Slug = o.Slug,
            Location = o.Location,
            DisplayAddress = o.DisplayAddress,
            ExternalPlaceId = o.ExternalPlaceId,
            Phone = o.Phone,
            ContactEmail = o.ContactEmail,
            IndustryType = o.IndustryType,
            TaxIdentificationNumber = o.TaxIdentificationNumber,
            LogoUrl = o.LogoUrl,
            CoverImageUrl = o.CoverImageUrl,
            LocationNote = o.LocationNote,
            BusinessHours = o.BusinessHours,
            RequiredFinderContractFields = o.RequiredFinderContractFields,
            RequiredOwnerContractFields = o.RequiredOwnerContractFields,
            Status = o.Status.ToString(),
            CreatedAt = o.CreatedAt,
        }).ToList();

        return (items, total);
    }
}
