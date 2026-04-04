using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Usecases.Organizations;

public sealed record OrganizationSettingResult
{
    public required List<FinderContactField> RequiredFinderContactFields { get; init; }
    public required List<FormFieldDefinition> RequiredOwnerFormFields { get; init; }
}
