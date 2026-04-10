using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.GetOrganizationSetting;

public sealed record GetOrganizationSettingQuery(Guid OrgId) : IRequest<OrganizationSettingResult>;
