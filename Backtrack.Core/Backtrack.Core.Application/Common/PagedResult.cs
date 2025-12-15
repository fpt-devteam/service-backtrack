namespace Backtrack.Core.Application.Common;

public record PagedResult<T>(int Total, List<T> Items);