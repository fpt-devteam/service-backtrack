using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Handovers.StaffConfirmHandover;

public sealed class StaffConfirmHandoverHandler(
    IHandoverRepository handoverRepository,
    IMembershipRepository membershipRepository,
    IEventPublisher eventPublisher) : IRequestHandler<StaffConfirmHandoverCommand, HandoverResult>
{
    public async Task<HandoverResult> Handle(StaffConfirmHandoverCommand command, CancellationToken cancellationToken)
    {
        var handover = await handoverRepository.GetByIdWithExtensionAsync(command.HandoverId, cancellationToken)
            ?? throw new NotFoundException(HandoverErrors.NotFound);

        if (handover is not OrgHandover orgHandover)
        {
            throw new ValidationException(HandoverErrors.InvalidHandoverType);
        }

        if (handover.Status == HandoverStatus.Confirmed)
        {
            throw new ValidationException(HandoverErrors.AlreadyConfirmed);
        }

        if (handover.Status == HandoverStatus.Expired)
        {
            throw new ValidationException(HandoverErrors.AlreadyExpired);
        }

        // Verify user is staff of the organization
        var membership = await membershipRepository.GetByOrgAndUserAsync(
            orgHandover.OrgId, command.UserId, cancellationToken);
        if (membership == null)
        {
            throw new ForbiddenException(HandoverErrors.StaffNotAuthorized);
        }

        // Verify owner has confirmed
        if (!orgHandover.OwnerVerified)
        {
            throw new ValidationException(HandoverErrors.OwnerNotConfirmed);
        }

        // Confirm the handover
        handover.Status = HandoverStatus.Confirmed;
        handover.ConfirmedAt = DateTimeOffset.UtcNow;
        handover.UpdatedAt = DateTimeOffset.UtcNow;
        orgHandover.StaffConfirmedAt = DateTimeOffset.UtcNow;
        orgHandover.UpdatedAt = DateTimeOffset.UtcNow;

        await handoverRepository.SaveChangesAsync();

        // Get finder post to publish event
        var handoverWithPosts = await handoverRepository.GetByIdWithPostsAsync(command.HandoverId, cancellationToken);

        // Publish event for post closure and points awarding
        var finderId = (handoverWithPosts as OrgHandover)?.FinderPost?.AuthorId ?? command.UserId ?? string.Empty;
        await eventPublisher.PublishHandoverConfirmedAsync(new HandoverConfirmedIntegrationEvent
        {
            HandoverId = handover.Id,
            FinderPostId = orgHandover.FinderPostId,
            OwnerPostId = null,
            FinderId = finderId,
            EventTimestamp = DateTimeOffset.UtcNow
        });

        return new HandoverResult
        {
            Id = handover.Id,
            Type = "Org",
            FinderPostId = orgHandover.FinderPostId,
            OwnerPostId = null,
            Status = handover.Status.ToString(),
            ConfirmedAt = handover.ConfirmedAt,
            ExpiresAt = handover.ExpiresAt,
            CreatedAt = handover.CreatedAt,
            OrgExtension = new HandoverOrgExtensionResult
            {
                Id = orgHandover.Id,
                OrgId = orgHandover.OrgId,
                StaffId = orgHandover.StaffId,
                OwnerVerified = orgHandover.OwnerVerified,
                OwnerFormData = orgHandover.OwnerFormData,
                StaffConfirmedAt = orgHandover.StaffConfirmedAt,
                OwnerConfirmedAt = orgHandover.OwnerConfirmedAt
            }
        };
    }
}
