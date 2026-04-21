using MediatR;
using System.Text.Json.Serialization;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetC2CReturnReportsByPartnerId;

public sealed record GetC2CReturnReportsByPartnerIdQuery : IRequest<PagedResult<C2CReturnReportResult>>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    public string PartnerId { get; init; } = string.Empty;
}
