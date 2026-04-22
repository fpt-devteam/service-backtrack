using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetC2CReturnReportByPosts;

public sealed record GetC2CReturnReportByPostsQuery : IRequest<C2CReturnReportResult>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    public Guid FinderPostId { get; init; }
    public Guid OwnerPostId { get; init; }
}
