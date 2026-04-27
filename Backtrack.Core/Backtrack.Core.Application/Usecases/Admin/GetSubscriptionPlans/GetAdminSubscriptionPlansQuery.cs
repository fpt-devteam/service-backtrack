using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetSubscriptionPlans;

public sealed record GetAdminSubscriptionPlansQuery : IRequest<AdminSubscriptionPlansResult>
{
    public required string AdminUserId { get; init; }
}
