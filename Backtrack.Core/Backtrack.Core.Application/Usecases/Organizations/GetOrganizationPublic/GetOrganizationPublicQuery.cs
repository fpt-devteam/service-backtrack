using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetOrganizationPublic;

public sealed record GetOrganizationPublicQuery(string Slug) : IRequest<OrganizationResult>;
