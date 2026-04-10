using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.CheckSlugExist;

public sealed record CheckSlugExistQuery(string Slug) : IRequest<bool>;
