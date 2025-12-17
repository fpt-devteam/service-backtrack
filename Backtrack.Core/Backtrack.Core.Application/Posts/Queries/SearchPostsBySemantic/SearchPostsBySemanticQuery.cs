using Backtrack.Core.Application.Common;
using Backtrack.Core.Application.Posts.Common;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Posts.Queries.SearchPostsBySemantic;

public sealed record SearchPostsBySemanticQuery(
    string SearchText,
    PagedQuery PagedQuery,
    PostType? PostType = null,
    double? Latitude = null,
    double? Longitude = null,
    double? RadiusInKm = null) : IRequest<PagedResult<PostSemanticSearchResult>>;
