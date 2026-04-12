using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetGrowthChart;

public sealed class GetGrowthChartHandler(
    IUserRepository         userRepository,
    IOrganizationRepository organizationRepository) : IRequestHandler<GetGrowthChartQuery, GrowthChartResult>
{
    public async Task<GrowthChartResult> Handle(GetGrowthChartQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var raw = query.Entity switch
        {
            GrowthChartEntity.Users => await userRepository.GetGrowthChartAsync(query.Months, cancellationToken),
            GrowthChartEntity.Orgs  => await organizationRepository.GetGrowthChartAsync(query.Months, cancellationToken),
            _ => []
        };

        return new GrowthChartResult
        {
            Entity = query.Entity.ToString(),
            Months = query.Months,
            Points = raw.Select(r => new PeriodCount(r.Period, r.Count)).ToList()
        };
    }
}
