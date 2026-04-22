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
}
