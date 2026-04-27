using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetOrganizations;

public sealed record GetOrganizationsQuery : IRequest<OrganizationsResult>
{
    public required string AdminUserId { get; init; }
    public string? Search { get; init; }
    public OrganizationStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    /// <summary>createdAt | name</summary>
    public string SortBy { get; init; } = "createdAt";
    /// <summary>asc | desc</summary>
    public string SortOrder { get; init; } = "desc";
}
