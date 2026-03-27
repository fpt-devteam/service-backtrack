using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
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
            if (handoverWithPosts.FinderPost?.AuthorId == query.UserId)
                isAuthorized = true;
            else if (handoverWithPosts.OwnerPost?.AuthorId == query.UserId)
                isAuthorized = true;
            else if (handoverWithPosts.OrgExtension?.StaffId == query.UserId)
                isAuthorized = true;
        }

        if (!isAuthorized)
        {
            throw new ForbiddenException(new Error("NotAuthorized", "You are not authorized to view this handover."));
        }

        return new HandoverResult
        {
            Id = handover.Id,
            Type = handover.Type.ToString(),
            FinderPostId = handover.FinderPostId,
            OwnerPostId = handover.OwnerPostId,
            Status = handover.Status.ToString(),
            ConfirmedAt = handover.ConfirmedAt,
            ExpiresAt = handover.ExpiresAt,
            CreatedAt = handover.CreatedAt,
            OrgExtension = handover.OrgExtension != null ? new HandoverOrgExtensionResult
            {
                Id = handover.OrgExtension.Id,
                OrgId = handover.OrgExtension.OrgId,
                StaffId = handover.OrgExtension.StaffId,
                OwnerVerified = handover.OrgExtension.OwnerVerified,
                OwnerFormData = handover.OrgExtension.OwnerFormData,
                StaffConfirmedAt = handover.OrgExtension.StaffConfirmedAt,
                OwnerConfirmedAt = handover.OrgExtension.OwnerConfirmedAt
            } : null
        };
    }
}
