using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.Organizations;

public sealed record OrganizationSettingResult
{
    public required List<FinderContactField> RequiredFinderContactFields { get; init; }
}
