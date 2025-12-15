using System.ComponentModel.DataAnnotations;

namespace Backtrack.Core.Contract.Common;

public record PagedRequest
{
    [Required]
    public int Page { get; init; } = 1;
    [Required]
    public int PageSize { get; init; } = 20;
}
