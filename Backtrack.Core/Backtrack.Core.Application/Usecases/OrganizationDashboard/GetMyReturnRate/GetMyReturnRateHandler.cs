using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationDashboard.GetMyReturnRate;

public sealed class GetMyReturnRateHandler(
    IMembershipRepository membershipRepository,
    IPostRepository       postRepository)
    : IRequestHandler<GetMyReturnRateQuery, MyReturnRateResult>
{
    public async Task<MyReturnRateResult> Handle(
        GetMyReturnRateQuery query, CancellationToken cancellationToken)
    {
        _ = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        var returned  = await postRepository.CountAsync(new PostFilters { AuthorId = query.UserId, OrganizationId = query.OrgId, Status = PostStatus.Returned  }, cancellationToken);
        var inStorage = await postRepository.CountAsync(new PostFilters { AuthorId = query.UserId, OrganizationId = query.OrgId, Status = PostStatus.InStorage }, cancellationToken);
        var expired   = await postRepository.CountAsync(new PostFilters { AuthorId = query.UserId, OrganizationId = query.OrgId, Status = PostStatus.Expired   }, cancellationToken);
        var total     = await postRepository.CountAsync(new PostFilters { AuthorId = query.UserId, OrganizationId = query.OrgId                               }, cancellationToken);
        var other     = total - returned - inStorage - expired;
        var returnRate = total > 0 ? Math.Round((double)returned / total * 100, 1) : 0.0;

        return new MyReturnRateResult
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
