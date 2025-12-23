using System.ComponentModel.DataAnnotations;

namespace Backtrack.Core.WebApi.Contracts.Common;

public record PagedRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
