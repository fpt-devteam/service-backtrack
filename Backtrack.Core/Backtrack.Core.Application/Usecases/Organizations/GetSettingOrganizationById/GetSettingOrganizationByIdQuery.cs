using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetSettingOrganizationById;

public sealed record GetSettingOrganizationByIdQuery(Guid OrgId) : IRequest<OrganizationSettingResult>;
