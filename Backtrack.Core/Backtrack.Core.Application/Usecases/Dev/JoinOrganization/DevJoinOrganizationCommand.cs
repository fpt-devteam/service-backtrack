using Backtrack.Core.Domain.Constants;
using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.Dev.JoinOrganization;

public sealed record DevJoinOrganizationCommand : IRequest<DevJoinOrganizationResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;
    public required Guid OrganizationId { get; init; }
    public required MembershipRole Role { get; init; }
}
