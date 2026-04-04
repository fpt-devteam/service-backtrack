using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Handovers.OwnerConfirmHandover;

public sealed record OwnerConfirmHandoverCommand : IRequest<HandoverResult>
{
    [JsonIgnore]
    public string? UserId { get; init; }

    [JsonIgnore]
    public Guid HandoverId { get; init; }

    public Dictionary<string, string>? OwnerFormData { get; init; }
}
