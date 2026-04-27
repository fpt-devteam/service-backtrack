using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetSubscriptionPlans;

public sealed class GetAdminSubscriptionPlansHandler(
    IUserRepository userRepository,
    ISubscriptionPlanRepository planRepository)
    : IRequestHandler<GetAdminSubscriptionPlansQuery, AdminSubscriptionPlansResult>
{
    public async Task<AdminSubscriptionPlansResult> Handle(
        GetAdminSubscriptionPlansQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var all = await planRepository.GetAllForAdminAsync(cancellationToken);

        static AdminSubscriptionPlanItem Map(Domain.Entities.SubscriptionPlan p) => new()
        {
            Id              = p.Id,
            Name            = p.Name,
            Price           = p.Price,
            Currency        = p.Currency,
            BillingInterval = p.BillingInterval,
            Features        = p.Features,
            ProviderPriceId = p.ProviderPriceId,
            IsActive        = p.IsActive,
            CreatedAt       = p.CreatedAt,
        };

        return new AdminSubscriptionPlansResult
        {
            User         = all.Where(p => p.SubscriberType == SubscriberType.User).Select(Map).ToList(),
            Organization = all.Where(p => p.SubscriberType == SubscriberType.Organization).Select(Map).ToList(),
        };
    }
}
