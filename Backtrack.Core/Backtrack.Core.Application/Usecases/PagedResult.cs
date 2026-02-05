namespace Backtrack.Core.Application.Usecases;

public record PagedResult<T>(int Total, List<T> Items);