using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetMyOrganizations;

public sealed record GetMyOrganizationsQuery(string UserId) : IRequest<List<MyOrganizationResult>>;
