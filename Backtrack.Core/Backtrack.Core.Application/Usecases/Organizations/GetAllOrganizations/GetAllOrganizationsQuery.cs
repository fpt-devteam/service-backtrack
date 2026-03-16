using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetAllOrganizations;

public sealed record GetAllOrganizationsQuery(int Page = 1, int PageSize = 20)
    : IRequest<(List<OrganizationResult> Items, int Total)>;
