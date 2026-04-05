using System.Text.Json.Serialization;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetC2CReturnReportsByUserId;

public sealed record GetC2CReturnReportsByUserIdQuery : IRequest<PagedResult<C2CReturnReportResult>>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public ReturnReportStatus? Status { get; init; }
}
