using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.ReturnReport.InitiateC2CReturnReport;
using Backtrack.Core.Application.Usecases.Users;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.InitiateC2CReturnReport;

public sealed class InitiateC2CReturnReportHandler(
    IC2CReturnReportRepository returnReportRepository,
    IPostRepository postRepository,
    IUserRepository userRepository,
    IEventPublisher eventPublisher) : IRequestHandler<InitiateC2CReturnReportCommand, C2CReturnReportResult>
{
    public async Task<C2CReturnReportResult> Handle(InitiateC2CReturnReportCommand command, CancellationToken cancellationToken)
    {

        Post? finderPost = null;
        Post? ownerPost = null;

        // Load and validate finder post (must be Found type)
        if (command.FinderPostId.HasValue)
        {
            finderPost = await postRepository.GetByIdAsync(command.FinderPostId.Value)
                ?? throw new NotFoundException(ReturnReportErrors.FinderPostNotFound);

            if (finderPost.PostType != PostType.Found)
                throw new ValidationException(ReturnReportErrors.PostTypeMismatch);
        }

        // Load and validate owner post (must be Lost type)
        if (command.OwnerPostId.HasValue)
        {
            ownerPost = await postRepository.GetByIdAsync(command.OwnerPostId.Value)
                ?? throw new NotFoundException(ReturnReportErrors.OwnerPostNotFound);

            if (ownerPost.PostType != PostType.Lost)
                throw new ValidationException(ReturnReportErrors.PostTypeMismatch);
        }

        // Determine initiator's role based on post ownership or explicit body fields
        bool initiatorIsFinder = DetermineInitiatorRole(command.InitiatorId, finderPost, ownerPost, command.FinderId, command.OwnerId);

        string resolvedFinderId;
        string resolvedOwnerId;

        if (initiatorIsFinder)
        {
            resolvedFinderId = command.InitiatorId;

            if (ownerPost != null)
            {
                if (ownerPost.AuthorId == command.InitiatorId)
                    throw new ValidationException(ReturnReportErrors.CannotReturnReportOwnPost);

                resolvedOwnerId = ownerPost.AuthorId;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(command.OwnerId))
                    throw new ValidationException(ReturnReportErrors.OwnerIdRequired);

                if (command.OwnerId == command.InitiatorId)
                    throw new ValidationException(ReturnReportErrors.CannotReturnReportOwnPost);

                resolvedOwnerId = command.OwnerId;
            }
        }
        else
        {
            // Initiator is the Owner
            resolvedOwnerId = command.InitiatorId;

            if (finderPost != null)
            {
                if (finderPost.AuthorId == command.InitiatorId)
                    throw new ValidationException(ReturnReportErrors.CannotReturnReportOwnPost);

                resolvedFinderId = finderPost.AuthorId;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(command.FinderId))
                    throw new ValidationException(ReturnReportErrors.FinderIdRequired);

                if (command.FinderId == command.InitiatorId)
                    throw new ValidationException(ReturnReportErrors.CannotReturnReportOwnPost);

                resolvedFinderId = command.FinderId;
            }
        }

        var finder = await userRepository.GetByIdAsync(resolvedFinderId)
            ?? throw new NotFoundException(ReturnReportErrors.FinderNotFound);

        var owner = await userRepository.GetByIdAsync(resolvedOwnerId)
            ?? throw new NotFoundException(ReturnReportErrors.OwnerNotFound);

        // Only block the initiator's own post — the counterpart's post remains free for others to use
        if (initiatorIsFinder && command.FinderPostId.HasValue)
        {
            var finderPostTaken = await returnReportRepository.ExistsActiveReturnReportForFinderPostAsync(
                command.FinderPostId.Value, cancellationToken);
            if (finderPostTaken)
                throw new ConflictException(ReturnReportErrors.FinderPostAlreadyInReport);
        }

        if (!initiatorIsFinder && command.OwnerPostId.HasValue)
        {
            var ownerPostTaken = await returnReportRepository.ExistsActiveReturnReportForOwnerPostAsync(
                command.OwnerPostId.Value, cancellationToken);
            if (ownerPostTaken)
                throw new ConflictException(ReturnReportErrors.OwnerPostAlreadyInReport);
        }

        var returnReport = new C2CReturnReport
        {
            Id = Guid.NewGuid(),
            FinderId = resolvedFinderId,
            OwnerId = resolvedOwnerId,
            FinderPostId = command.FinderPostId,
            OwnerPostId = command.OwnerPostId,
            Status = C2CReturnReportStatus.Ongoing,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        await returnReportRepository.CreateAsync(returnReport);
        await returnReportRepository.SaveChangesAsync();

        await eventPublisher.PublishReturnReportSyncAsync(new ReturnReportSyncIntegrationEvent
        {
            C2CReturnReportId  = returnReport.Id,
            FinderId           = finder.Id,
            FinderDisplayName  = finder.DisplayName,
            FinderAvatarUrl    = finder.AvatarUrl,
            FinderEmail        = finder.Email,
            OwnerId            = owner.Id,
            OwnerDisplayName   = owner.DisplayName,
            OwnerAvatarUrl     = owner.AvatarUrl,
            OwnerEmail         = owner.Email,
            FinderPostId       = returnReport.FinderPostId,
            FinderPostType     = finderPost?.PostType.ToString(),
            OwnerPostId        = returnReport.OwnerPostId,
            OwnerPostType      = ownerPost?.PostType.ToString(),
            Status             = returnReport.Status.ToString(),
            ActivatedByRole    = null,
            ConfirmedAt        = returnReport.ConfirmedAt,
            ExpiresAt          = returnReport.ExpiresAt,
            CreatedAt          = returnReport.CreatedAt,
            EventTimestamp     = DateTimeOffset.UtcNow,
        });

        return new C2CReturnReportResult
        {
            Id = returnReport.Id,
            Finder = finder.ToUserResult(),
            Owner = owner.ToUserResult(),
            FinderPost = finderPost?.ToPostResult(),
            OwnerPost = ownerPost?.ToPostResult(),
            Status = returnReport.Status.ToString(),
            ActivatedByRole = null,
            ConfirmedAt = returnReport.ConfirmedAt,
            ExpiresAt = returnReport.ExpiresAt,
            CreatedAt = returnReport.CreatedAt
        };
    }

    /// <summary>
    /// Determines whether the initiator is the Finder or Owner.
    /// - If initiator authored the Found post → Finder.
    /// - If initiator authored the Lost post → Owner.
    /// - If only finderPost provided and initiator is NOT the author → initiator is Owner (referencing counterpart's Found post).
    /// - If only ownerPost provided and initiator is NOT the author → initiator is Finder (referencing counterpart's Lost post).
    /// - If both posts provided but initiator authored neither → NotParticipant.
    /// - If no posts provided: OwnerId in body → Finder; FinderId in body → Owner.
    /// </summary>
    private static bool DetermineInitiatorRole(string initiatorId, Post? finderPost, Post? ownerPost, string? finderId, string? ownerId)
    {
        if (finderPost != null && finderPost.AuthorId == initiatorId)
            return true;  // initiator owns the Found post → Finder

        if (ownerPost != null && ownerPost.AuthorId == initiatorId)
            return false; // initiator owns the Lost post → Owner

        // Only finderPost provided, initiator is not its author → initiator is the Owner
        if (finderPost != null && ownerPost == null)
            return false;

        // Only ownerPost provided, initiator is not its author → initiator is the Finder
        if (ownerPost != null && finderPost == null)
            return true;

        // Both posts provided but initiator authored neither → not a participant
        if (finderPost != null && ownerPost != null)
            throw new ForbiddenException(ReturnReportErrors.NotParticipant);

        // No posts provided: determine role from which explicit ID is in the body
        // OwnerId present → initiator is Finder; FinderId present → initiator is Owner
        if (!string.IsNullOrWhiteSpace(ownerId))
            return true;

        if (!string.IsNullOrWhiteSpace(finderId))
            return false;

        throw new ValidationException(new Error("CounterpartRequired", "Provide either OwnerId (if you are the finder) or FinderId (if you are the owner)."));
    }
}
