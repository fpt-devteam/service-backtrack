using MediatR;

namespace Backtrack.Core.Application.Usecases.StripeWebhooks.HandleStripeEvent;

public sealed record HandleStripeEventCommand : IRequest
{
    public required string Json { get; init; }
    public required string Signature { get; init; }
}
