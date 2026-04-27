using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.UpdateSubscriptionPlanFeatures;

public sealed class UpdateSubscriptionPlanFeaturesHandler(
    IUserRepository userRepository,
    ISubscriptionPlanRepository planRepository)
    : IRequestHandler<UpdateSubscriptionPlanFeaturesCommand, UpdateSubscriptionPlanFeaturesResult>
{
    public async Task<UpdateSubscriptionPlanFeaturesResult> Handle(
        UpdateSubscriptionPlanFeaturesCommand command, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(command.AdminUserId!);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var plan = await planRepository.GetByIdAsync(command.PlanId, isTrack: true)
            ?? throw new NotFoundException(SubscriptionErrors.PlanNotFound);

        plan.Features = command.Features;
        planRepository.Update(plan);
        await planRepository.SaveChangesAsync();

        return new UpdateSubscriptionPlanFeaturesResult
        {
            Id       = plan.Id,
            Name     = plan.Name,
            Features = plan.Features,
        };
    }
}
