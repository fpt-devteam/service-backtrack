using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetOrganizationById;

public sealed record GetOrganizationByIdQuery(Guid OrgId, string UserId) : IRequest<OrganizationResult>;
