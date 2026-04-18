using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Subcategories.GetSubcategories;

public sealed class GetSubcategoriesHandler(ISubcategoryRepository subcategoryRepository)
    : IRequestHandler<GetSubcategoriesQuery, List<SubcategoryResult>>
{
    public async Task<List<SubcategoryResult>> Handle(GetSubcategoriesQuery query, CancellationToken cancellationToken)
    {
        var subcategories = await subcategoryRepository.GetAllActiveAsync(cancellationToken);

        if (query.Category is not null && Enum.TryParse<ItemCategory>(query.Category, ignoreCase: true, out var category))
            subcategories = subcategories.Where(s => s.Category == category).ToList();

        return subcategories
            .Select(s => new SubcategoryResult
            {
                Id           = s.Id,
                Category     = s.Category,
                Code         = s.Code,
                Name         = s.Name,
                DisplayOrder = s.DisplayOrder,
            })
            .ToList();
    }
}
