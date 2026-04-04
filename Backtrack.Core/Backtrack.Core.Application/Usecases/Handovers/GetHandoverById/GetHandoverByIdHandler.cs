using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Handovers.GetHandoverById;

public sealed class GetHandoverByIdHandler(
    IHandoverRepository handoverRepository) : IRequestHandler<GetHandoverByIdQuery, HandoverResult>
{
    public async Task<HandoverResult> Handle(GetHandoverByIdQuery query, CancellationToken cancellationToken)
    {
        var handover = await handoverRepository.GetByIdWithExtensionAsync(query.HandoverId, cancellationToken)
            ?? throw new NotFoundException(HandoverErrors.NotFound);

        // Check if user is authorized to view this handover
        // User must be the finder post author, owner post author, or org staff
        var isAuthorized = false;

        var handoverWithPosts = await handoverRepository.GetByIdWithPostsAsync(query.HandoverId, cancellationToken);
        if (handoverWithPosts != null)
        {
            if (handoverWithPosts is P2PHandover p2p && p2p.FinderPost?.AuthorId == query.UserId)
                isAuthorized = true;
            else if (handoverWithPosts is P2PHandover p2pOwner && p2pOwner.OwnerPost?.AuthorId == query.UserId)
                isAuthorized = true;
            else if (handoverWithPosts is OrgHandover org && org.StaffId == query.UserId)
                isAuthorized = true;
            else if (handoverWithPosts is OrgHandover orgFinder && orgFinder.FinderPost?.AuthorId == query.UserId)
                isAuthorized = true;
        }

        if (!isAuthorized)
        {
            throw new ForbiddenException(new Error("NotAuthorized", "You are not authorized to view this handover."));
        }

        return new HandoverResult
        {
            Id = handover.Id,
            Type = handover is OrgHandover ? "Org" : "P2P",
            FinderPostId = handover is P2PHandover p2pResult ? p2pResult.FinderPostId : ((OrgHandover)handover).FinderPostId,
            OwnerPostId = handover is P2PHandover p2pOwnerResult ? p2pOwnerResult.OwnerPostId : null,
            Status = handover.Status.ToString(),
            ConfirmedAt = handover.ConfirmedAt,
            ExpiresAt = handover.ExpiresAt,
            CreatedAt = handover.CreatedAt,
            OrgExtension = handover is OrgHandover orgResult ? new HandoverOrgExtensionResult
            {
                Id = orgResult.Id,
                OrgId = orgResult.OrgId,
                StaffId = orgResult.StaffId,
                OwnerVerified = orgResult.OwnerVerified,
                OwnerFormData = orgResult.OwnerFormData,
                StaffConfirmedAt = orgResult.StaffConfirmedAt,
                OwnerConfirmedAt = orgResult.OwnerConfirmedAt
            } : null
        };
    }
}
