using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.GetDashboardInventory;

public sealed record GetDashboardInventoryQuery : IRequest<PagedResult<DashboardInventoryItem>>
{
    [JsonIgnore] public string UserId { get; init; } = string.Empty;
    [JsonIgnore] public Guid   OrgId  { get; init; }

    /// <summary>"me" filters to the caller's own items; omit for all staff.</summary>
    public string? StaffId   { get; init; }
    public int     Page      { get; init; } = 1;
    public int     PageSize  { get; init; } = 10;
}
