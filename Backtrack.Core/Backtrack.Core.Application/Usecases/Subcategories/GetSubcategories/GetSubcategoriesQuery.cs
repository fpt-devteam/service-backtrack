using MediatR;

namespace Backtrack.Core.Application.Usecases.Subcategories.GetSubcategories;

public sealed record GetSubcategoriesQuery : IRequest<List<SubcategoryResult>>;
