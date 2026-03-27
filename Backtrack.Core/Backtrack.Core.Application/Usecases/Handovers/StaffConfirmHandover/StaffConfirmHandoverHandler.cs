using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
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

        if (handover.Type != HandoverType.Org)
        {
            throw new ValidationException(HandoverErrors.InvalidHandoverType);
        }

        if (handover.OrgExtension == null)
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
            handover.OrgExtension.OrgId, command.UserId, cancellationToken);
        if (membership == null)
        {
            throw new ForbiddenException(HandoverErrors.StaffNotAuthorized);
        }

        // Verify owner has confirmed
        if (!handover.OrgExtension.OwnerVerified)
        {
            throw new ValidationException(HandoverErrors.OwnerNotConfirmed);
        }

        // Confirm the handover
        handover.Status = HandoverStatus.Confirmed;
        handover.ConfirmedAt = DateTimeOffset.UtcNow;
        handover.UpdatedAt = DateTimeOffset.UtcNow;
        handover.OrgExtension.StaffConfirmedAt = DateTimeOffset.UtcNow;
        handover.OrgExtension.UpdatedAt = DateTimeOffset.UtcNow;

        await handoverRepository.SaveChangesAsync();

        // Get finder post to publish event
        var handoverWithPosts = await handoverRepository.GetByIdWithPostsAsync(command.HandoverId, cancellationToken);

        // Publish event for post closure and points awarding
        var finderId = handoverWithPosts?.FinderPost?.AuthorId ?? command.UserId ?? string.Empty;
        await eventPublisher.PublishHandoverConfirmedAsync(new HandoverConfirmedIntegrationEvent
        {
            HandoverId = handover.Id,
            FinderPostId = handover.FinderPostId,
            OwnerPostId = handover.OwnerPostId,
            FinderId = finderId,
            EventTimestamp = DateTimeOffset.UtcNow
        });

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
            OrgExtension = new HandoverOrgExtensionResult
            {
                Id = handover.OrgExtension.Id,
                OrgId = handover.OrgExtension.OrgId,
                StaffId = handover.OrgExtension.StaffId,
                OwnerVerified = handover.OrgExtension.OwnerVerified,
                OwnerFormData = handover.OrgExtension.OwnerFormData,
                StaffConfirmedAt = handover.OrgExtension.StaffConfirmedAt,
                OwnerConfirmedAt = handover.OrgExtension.OwnerConfirmedAt
            }
        };
    }
}
