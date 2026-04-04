using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.Organizations;

public sealed record OrganizationSettingResult
{
    public required List<OrgContractField> RequiredFinderContractFields { get; init; }
    public required List<OrgContractField> RequiredOwnerContractFields { get; init; }
}
