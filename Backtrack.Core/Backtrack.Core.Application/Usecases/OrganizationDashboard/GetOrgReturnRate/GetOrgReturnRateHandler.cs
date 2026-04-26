using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetOrgReturnRate;

public sealed class GetOrgReturnRateHandler(
    IMembershipRepository membershipRepository,
    IPostRepository       postRepository)
    : IRequestHandler<GetOrgReturnRateQuery, OrgReturnRateResult>
{
    public async Task<OrgReturnRateResult> Handle(
        GetOrgReturnRateQuery query, CancellationToken cancellationToken)
    {
        _ = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        var returned  = await postRepository.CountAsync(new PostFilters { OrganizationId = query.OrgId, Status = PostStatus.Returned  }, cancellationToken);
        var inStorage = await postRepository.CountAsync(new PostFilters { OrganizationId = query.OrgId, Status = PostStatus.InStorage }, cancellationToken);
        var expired   = await postRepository.CountAsync(new PostFilters { OrganizationId = query.OrgId, Status = PostStatus.Expired   }, cancellationToken);
        var total     = await postRepository.CountAsync(new PostFilters { OrganizationId = query.OrgId                               }, cancellationToken);
        var other     = total - returned - inStorage - expired;
        var returnRate = total > 0 ? Math.Round((double)returned / total * 100, 1) : 0.0;

        return new OrgReturnRateResult
        {
            Returned   = returned,
            InStorage  = inStorage,
            Expired    = expired,
            Other      = other,
            Total      = total,
            ReturnRate = returnRate
        };
    }
}
