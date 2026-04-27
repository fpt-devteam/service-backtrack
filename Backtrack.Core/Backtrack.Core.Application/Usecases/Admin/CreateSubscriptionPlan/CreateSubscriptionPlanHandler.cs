using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.CreateSubscriptionPlan;

public sealed class CreateSubscriptionPlanHandler(
    IUserRepository userRepository,
    ISubscriptionPlanRepository planRepository)
    : IRequestHandler<CreateSubscriptionPlanCommand, CreateSubscriptionPlanResult>
{
    public async Task<CreateSubscriptionPlanResult> Handle(
        CreateSubscriptionPlanCommand command, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(command.AdminUserId!);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        var plan = await planRepository.CreateAsync(new SubscriptionPlan
        {
            Id               = Guid.NewGuid(),
            Name             = command.Name,
            Price            = command.Price,
            Currency         = command.Currency,
            BillingInterval  = command.BillingInterval,
            SubscriberType   = command.SubscriberType,
            Features         = command.Features,
            ProviderPriceId  = command.ProviderPriceId,
            IsActive         = true,
        });

        await planRepository.SaveChangesAsync();

        return new CreateSubscriptionPlanResult
        {
            Id              = plan.Id,
            Name            = plan.Name,
            Price           = plan.Price,
            Currency        = plan.Currency,
            BillingInterval = plan.BillingInterval,
            SubscriberType  = plan.SubscriberType,
            Features        = plan.Features,
            ProviderPriceId = plan.ProviderPriceId,
            IsActive        = plan.IsActive,
            CreatedAt       = plan.CreatedAt,
        };
    }
}
