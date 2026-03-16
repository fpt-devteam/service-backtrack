using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetOrganizationPublic;

public sealed record GetOrganizationPublicQuery(Guid OrgId) : IRequest<OrganizationResult>;
