using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetOrganization;

public sealed record GetOrganizationQuery(Guid OrgId, string UserId) : IRequest<OrganizationResult>;
