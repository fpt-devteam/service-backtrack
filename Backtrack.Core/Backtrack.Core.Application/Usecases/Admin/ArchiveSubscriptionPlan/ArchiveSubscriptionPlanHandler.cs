using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.ArchiveSubscriptionPlan;

public sealed class ArchiveSubscriptionPlanHandler(
    IUserRepository userRepository,
    ISubscriptionPlanRepository planRepository,
    ISubscriptionRepository subscriptionRepository)
    : IRequestHandler<ArchiveSubscriptionPlanCommand, ArchiveSubscriptionPlanResult>
{
    public async Task<ArchiveSubscriptionPlanResult> Handle(
        ArchiveSubscriptionPlanCommand command, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(command.AdminUserId!);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var plan = await planRepository.GetByIdAsync(command.PlanId, isTrack: true)
            ?? throw new NotFoundException(SubscriptionErrors.PlanNotFound);

        if (!plan.IsActive)
            return new ArchiveSubscriptionPlanResult { Id = plan.Id, Name = plan.Name, IsActive = false };

        if (await subscriptionRepository.HasActiveByPlanIdAsync(command.PlanId, cancellationToken))
            throw new ConflictException(SubscriptionErrors.PlanHasActiveSubscriptions);

        plan.IsActive = false;
        planRepository.Update(plan);
        await planRepository.SaveChangesAsync();

        return new ArchiveSubscriptionPlanResult { Id = plan.Id, Name = plan.Name, IsActive = false };
    }
}
