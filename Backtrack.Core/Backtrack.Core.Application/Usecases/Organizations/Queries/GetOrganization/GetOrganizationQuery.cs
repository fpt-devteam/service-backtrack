using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.Queries.GetOrganization;

public sealed record GetOrganizationQuery(Guid OrgId, string UserId) : IRequest<OrganizationResult>;
