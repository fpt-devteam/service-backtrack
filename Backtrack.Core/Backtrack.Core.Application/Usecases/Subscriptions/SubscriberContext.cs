using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.Subscriptions;

/// <summary>Resolved by controllers; passed to subscription commands/queries.</summary>
public sealed record SubscriberContext
{
    public required SubscriberType SubscriberType { get; init; }
    public string? UserId { get; init; }
    public Guid? OrganizationId { get; init; }
}
