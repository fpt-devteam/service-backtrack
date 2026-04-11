using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.ReturnReport.CreateC2CReturnReport;
using Backtrack.Core.Application.Usecases.Users;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.CreateC2CReturnReport;

public sealed class CreateC2CReturnReportHandler(
    IC2CReturnReportRepository returnReportRepository,
    IPostRepository postRepository,
    IUserRepository userRepository) : IRequestHandler<CreateC2CReturnReportCommand, C2CReturnReportResult>
{
    public async Task<C2CReturnReportResult> Handle(CreateC2CReturnReportCommand command, CancellationToken cancellationToken)
    {
        if (command.Status != ReturnReportStatus.Draft && command.Status != ReturnReportStatus.Active)
            throw new ValidationException(new Error("InvalidStatus", "Status must be Draft or Active."));

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
            Status = command.Status,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        await returnReportRepository.CreateAsync(returnReport);
        await returnReportRepository.SaveChangesAsync();

        return new C2CReturnReportResult
        {
            Id = returnReport.Id,
            Finder = finder.ToUserResult(),
            Owner = owner.ToUserResult(),
            FinderPost = finderPost?.ToPostResult(),
            OwnerPost = ownerPost?.ToPostResult(),
            Status = returnReport.Status.ToString(),
            ConfirmedAt = returnReport.ConfirmedAt,
            ExpiresAt = returnReport.ExpiresAt,
            CreatedAt = returnReport.CreatedAt
        };
    }

    /// <summary>
    /// Determines whether the initiator is the Finder or Owner.
    /// Priority: if initiator owns the finder post → Finder; if initiator owns the owner post → Owner.
    /// When only one post is provided, the initiator must own that post.
    /// When no posts are provided, the presence of OwnerId implies Finder role, FinderId implies Owner role.
    /// </summary>
    private static bool DetermineInitiatorRole(string initiatorId, Post? finderPost, Post? ownerPost, string? finderId, string? ownerId)
    {
        if (finderPost != null && finderPost.AuthorId == initiatorId)
            return true;  // initiator owns the Found post → Finder

        if (ownerPost != null && ownerPost.AuthorId == initiatorId)
            return false; // initiator owns the Lost post → Owner

        // Neither post is owned by initiator
        if (finderPost != null || ownerPost != null)
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
