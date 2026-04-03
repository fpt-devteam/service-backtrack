using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Handovers.OwnerConfirmHandover;

public sealed class OwnerConfirmHandoverHandler(
    IHandoverRepository handoverRepository,
    IOrgFormTemplateRepository orgFormTemplateRepository,
    IEventPublisher eventPublisher) : IRequestHandler<OwnerConfirmHandoverCommand, HandoverResult>
{
    public async Task<HandoverResult> Handle(OwnerConfirmHandoverCommand command, CancellationToken cancellationToken)
    {
        var handover = await handoverRepository.GetByIdWithExtensionAsync(command.HandoverId, cancellationToken)
            ?? throw new NotFoundException(HandoverErrors.NotFound);

        if (handover.Status == HandoverStatus.Confirmed)
        {
            throw new ValidationException(HandoverErrors.AlreadyConfirmed);
        }

        if (handover.Status == HandoverStatus.Expired)
        {
            throw new ValidationException(HandoverErrors.AlreadyExpired);
        }

        // For P2P handovers, verify user is the owner post author
        if (handover is P2PHandover p2pHandover)
        {
            var handoverWithPosts = await handoverRepository.GetByIdWithPostsAsync(command.HandoverId, cancellationToken);
            if (handoverWithPosts is not P2PHandover p2pWithPosts || p2pWithPosts.OwnerPost?.AuthorId != command.UserId)
            {
                throw new ForbiddenException(new Error("NotOwner", "Only the owner can confirm this handover."));
            }

            // P2P handover is confirmed immediately by owner
            handover.Status = HandoverStatus.Confirmed;
            handover.ConfirmedAt = DateTimeOffset.UtcNow;
            handover.UpdatedAt = DateTimeOffset.UtcNow;

            await handoverRepository.SaveChangesAsync();

            // Publish event for post closure and points awarding
            var finderId = (handoverWithPosts as P2PHandover)?.FinderPost?.AuthorId ?? command.UserId ?? string.Empty;
            await eventPublisher.PublishHandoverConfirmedAsync(new HandoverConfirmedIntegrationEvent
            {
                HandoverId = handover.Id,
                FinderPostId = p2pHandover.FinderPostId,
                OwnerPostId = p2pHandover.OwnerPostId,
                FinderId = finderId,
                EventTimestamp = DateTimeOffset.UtcNow
            });
        }
        else if (handover is OrgHandover orgHandover)
        {
            // For Org handovers, validate form data against template
            var formTemplate = await orgFormTemplateRepository.GetByOrgIdAsync(
                orgHandover.OrgId, cancellationToken);

            if (formTemplate != null)
            {
                ValidateFormData(formTemplate.Fields, command.OwnerFormData);
            }

            // Store owner form data and mark as owner confirmed (pending staff confirmation)
            orgHandover.OwnerFormData = command.OwnerFormData;
            orgHandover.OwnerConfirmedAt = DateTimeOffset.UtcNow;
            orgHandover.OwnerVerified = true;
            orgHandover.UpdatedAt = DateTimeOffset.UtcNow;

            await handoverRepository.SaveChangesAsync();
        }
        else
        {
            throw new ValidationException(HandoverErrors.InvalidHandoverType);
        }

        return new HandoverResult
        {
            Id = handover.Id,
            Type = handover is OrgHandover ? "Org" : "P2P",
            FinderPostId = handover is P2PHandover p2p ? p2p.FinderPostId : ((OrgHandover)handover).FinderPostId,
            OwnerPostId = handover is P2PHandover p2pOwner ? p2pOwner.OwnerPostId : null,
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

    private static void ValidateFormData(
        List<Domain.ValueObjects.FormFieldDefinition> fields,
        Dictionary<string, string>? formData)
    {
        foreach (var field in fields.Where(f => f.Required))
        {
            if (formData == null ||
                !formData.TryGetValue(field.Key, out var value) ||
                string.IsNullOrWhiteSpace(value))
            {
                throw new ValidationException(HandoverErrors.MissingFormField(field.Key));
            }
        }
    }
}
