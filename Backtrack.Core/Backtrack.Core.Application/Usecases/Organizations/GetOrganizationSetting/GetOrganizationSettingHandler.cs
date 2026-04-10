using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetOrganizationSetting;

public sealed class GetOrganizationSettingHandler(IOrganizationRepository organizationRepository)
    : IRequestHandler<GetOrganizationSettingQuery, OrganizationSettingResult>
{
    public async Task<OrganizationSettingResult> Handle(GetOrganizationSettingQuery query, CancellationToken cancellationToken)
    {
        var org = await organizationRepository.GetByIdAsync(query.OrgId)
            ?? throw new NotFoundException(OrganizationErrors.NotFound);

        return new OrganizationSettingResult
        {
            RequiredFinderContractFields = org.RequiredFinderContractFields,
            RequiredOwnerContractFields  = org.RequiredOwnerContractFields
        };
    }
}
