using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Subscriptions.CreateCustomerPortalSession;

public sealed record CreateCustomerPortalSessionCommand : IRequest<CreateCustomerPortalSessionResult>
{
    public required Guid OrganizationId { get; init; }
    public required string ReturnUrl { get; init; }

    [JsonIgnore]
    public string? CallerId { get; init; }
}
