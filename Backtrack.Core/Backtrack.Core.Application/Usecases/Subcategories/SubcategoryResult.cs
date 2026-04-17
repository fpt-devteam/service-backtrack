using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Usecases.Subcategories;

public sealed record SubcategoryResult
{
    public required Guid Id { get; init; }
    public required ItemCategory Category { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required int DisplayOrder { get; init; }
}
