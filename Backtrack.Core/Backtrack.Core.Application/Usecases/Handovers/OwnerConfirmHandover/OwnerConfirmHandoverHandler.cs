using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
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
        if (handover.Type == HandoverType.P2P)
        {
            var handoverWithPosts = await handoverRepository.GetByIdWithPostsAsync(command.HandoverId, cancellationToken);
            if (handoverWithPosts?.OwnerPost?.AuthorId != command.UserId)
            {
                throw new ForbiddenException(new Error("NotOwner", "Only the owner can confirm this handover."));
            }

            // P2P handover is confirmed immediately by owner
            handover.Status = HandoverStatus.Confirmed;
            handover.ConfirmedAt = DateTimeOffset.UtcNow;
            handover.UpdatedAt = DateTimeOffset.UtcNow;

            await handoverRepository.SaveChangesAsync();

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
        }
        else if (handover.Type == HandoverType.Org)
        {
            // For Org handovers, validate form data against template
            if (handover.OrgExtension == null)
            {
                throw new ValidationException(HandoverErrors.InvalidHandoverType);
            }

            var formTemplate = await orgFormTemplateRepository.GetByOrgIdAsync(
                handover.OrgExtension.OrgId, cancellationToken);

            if (formTemplate != null)
            {
                ValidateFormData(formTemplate.Fields, command.OwnerFormData);
            }

            // Store owner form data and mark as owner confirmed (pending staff confirmation)
            handover.OrgExtension.OwnerFormData = command.OwnerFormData;
            handover.OrgExtension.OwnerConfirmedAt = DateTimeOffset.UtcNow;
            handover.OrgExtension.OwnerVerified = true;
            handover.OrgExtension.UpdatedAt = DateTimeOffset.UtcNow;

            await handoverRepository.SaveChangesAsync();
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
