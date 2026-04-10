using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetOrganizationBySlug;

public sealed record GetOrganizationBySlugQuery(string Slug) : IRequest<OrganizationResult>;
