using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetSettingOrganizationById;

public sealed class GetSettingOrganizationByIdHandler : IRequestHandler<GetSettingOrganizationByIdQuery, OrganizationSettingResult>
{
    private readonly IOrganizationRepository _organizationRepository;

    public GetSettingOrganizationByIdHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<OrganizationSettingResult> Handle(GetSettingOrganizationByIdQuery query, CancellationToken cancellationToken)
    {
        var org = await _organizationRepository.GetByIdAsync(query.OrgId);
        if (org is null)
        {
            throw new NotFoundException(OrganizationErrors.NotFound);
        }

        return new OrganizationSettingResult
        {
            RequiredFinderContactFields = org.RequiredFinderContactFields,
            RequiredOwnerFormFields = org.RequiredOwnerFormFields
        };
    }
}
