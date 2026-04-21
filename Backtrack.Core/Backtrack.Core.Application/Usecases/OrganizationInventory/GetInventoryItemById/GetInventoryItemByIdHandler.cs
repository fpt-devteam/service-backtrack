using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;
using Backtrack.Core.Application.Usecases.Posts;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.GetInventoryItemById;

public sealed class GetInventoryItemByIdHandler(
    IPostRepository postRepository,
    IMembershipRepository membershipRepository,
    IOrgReceiveReportRepository receiveReportRepository,
    IOrgReturnReportRepository returnReportRepository) : IRequestHandler<GetInventoryItemByIdQuery, InventoryItemResult>
{
    public async Task<InventoryItemResult> Handle(GetInventoryItemByIdQuery query, CancellationToken cancellationToken)
    {
        var membership = await membershipRepository.GetByOrgAndUserAsync(query.OrgId, query.UserId!, cancellationToken);
        if (membership is null) throw new ForbiddenException(MembershipErrors.NotAMember);

        var post = await postRepository.GetByIdAsync(query.PostId)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (post.OrganizationId != query.OrgId)
            throw new ForbiddenException(PostErrors.Forbidden);

        var receiveReport = await receiveReportRepository.GetByPostIdAsync(post.Id, cancellationToken);
        var returnReport  = await returnReportRepository.GetByPostIdAsync(post.Id, cancellationToken);

        return post.ToInventoryItemResult(receiveReport, returnReport);
    }
}
