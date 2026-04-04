using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Handovers.StaffConfirmHandover;

public sealed record StaffConfirmHandoverCommand : IRequest<HandoverResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public Guid HandoverId { get; init; }
}
