namespace Backtrack.Core.Application.Usecases.Admin.GetOrganizations;

/// <param name="Current">Current member count.</param>
/// <param name="Limit">Seat limit. null means unlimited (Max plan).</param>
public sealed record OrganizationCapacityResult(int Current, int? Limit);
