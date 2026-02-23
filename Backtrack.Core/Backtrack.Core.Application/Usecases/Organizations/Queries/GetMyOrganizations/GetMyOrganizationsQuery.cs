using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Queries.GetMyOrganizations;

public sealed record GetMyOrganizationsQuery(string UserId) : IRequest<List<MyOrganizationResult>>;
