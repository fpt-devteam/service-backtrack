using System.Text.Json.Serialization;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetPostMonthly;

public sealed record GetPostMonthlyQuery : IRequest<List<PostMonthlyResult>>
{
    [JsonIgnore]
    public string AdminUserId { get; init; } = string.Empty;
    public int Months { get; init; } = 12;
}
