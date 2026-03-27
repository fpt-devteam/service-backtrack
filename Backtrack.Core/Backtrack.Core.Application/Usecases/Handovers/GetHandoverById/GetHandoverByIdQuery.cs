using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Handovers.GetHandoverById;

public sealed record GetHandoverByIdQuery : IRequest<HandoverResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    public Guid HandoverId { get; init; }
}
