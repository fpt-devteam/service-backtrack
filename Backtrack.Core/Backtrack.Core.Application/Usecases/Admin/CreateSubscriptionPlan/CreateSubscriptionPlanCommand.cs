using Backtrack.Core.Domain.Constants;
using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Admin.CreateSubscriptionPlan;

public sealed record CreateSubscriptionPlanCommand : IRequest<CreateSubscriptionPlanResult>
{
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required string Currency { get; init; }
    public required SubscriptionBillingInterval BillingInterval { get; init; }
    public required SubscriberType SubscriberType { get; init; }
    public required string[] Features { get; init; }
    public required string ProviderPriceId { get; init; }

    [JsonIgnore]
    public string? AdminUserId { get; init; }
}
