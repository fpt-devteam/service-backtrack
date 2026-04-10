using Backtrack.Core.Application.Interfaces.Repositories;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Organizations.CheckSlugExist;

public sealed class CheckSlugExistHandler(IOrganizationRepository organizationRepository)
    : IRequestHandler<CheckSlugExistQuery, bool>
{
    public Task<bool> Handle(CheckSlugExistQuery query, CancellationToken cancellationToken)
        => organizationRepository.SlugExistsAsync(query.Slug, cancellationToken);
}
