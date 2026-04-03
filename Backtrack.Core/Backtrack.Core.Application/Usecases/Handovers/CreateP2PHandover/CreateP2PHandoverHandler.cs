using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Handovers.CreateP2PHandover;

public sealed class CreateP2PHandoverHandler(
    IHandoverRepository handoverRepository,
    IPostRepository postRepository,
    IUserRepository userRepository) : IRequestHandler<CreateP2PHandoverCommand, HandoverResult>
{
    public async Task<HandoverResult> Handle(CreateP2PHandoverCommand command, CancellationToken cancellationToken)
    {
        string resolvedOwnerId;

        if (command.FinderPostId.HasValue)
        {
            var finderPost = await postRepository.GetByIdAsync(command.FinderPostId.Value)
                ?? throw new NotFoundException(HandoverErrors.FinderPostNotFound);

            if (finderPost.AuthorId != command.FinderId)
            {
                throw new ForbiddenException(new Error("NotPostOwner", "You can only create handovers for your own posts."));
            }

            if (finderPost.PostType != PostType.Found)
            {
                throw new ValidationException(HandoverErrors.PostTypeMismatch);
            }
        }

        // Validate owner post if provided (Case 1, 2)
        if (command.OwnerPostId.HasValue)
        {
            var ownerPost = await postRepository.GetByIdAsync(command.OwnerPostId.Value)
                ?? throw new NotFoundException(HandoverErrors.OwnerPostNotFound);

            if (ownerPost.PostType != PostType.Lost)
            {
                throw new ValidationException(HandoverErrors.PostTypeMismatch);
            }

            if (ownerPost.AuthorId == command.FinderId)
            {
                throw new ValidationException(HandoverErrors.CannotHandoverOwnPost);
            }
            resolvedOwnerId = ownerPost.AuthorId;
        }
        else
        {
            // OwnerPostId is null (Case 3, 4) - OwnerId is required
            if (string.IsNullOrWhiteSpace(command.OwnerId))
            {
                throw new ValidationException(HandoverErrors.OwnerIdRequired);
            }

            if (command.OwnerId == command.FinderId)
            {
                throw new ValidationException(HandoverErrors.CannotHandoverOwnPost);
            }

            var owner = await userRepository.GetByIdAsync(command.OwnerId) ?? throw new NotFoundException(HandoverErrors.OwnerNotFound);
            resolvedOwnerId = command.OwnerId;
        }

        // Check if active handover already exists for this finder-owner pair
        var exists = await handoverRepository.ExistsActiveHandoverForPostsAsync(
            command.FinderPostId, command.OwnerPostId, cancellationToken);
        if (exists)
        {
            throw new ConflictException(HandoverErrors.AlreadyExists);
        }

        var handover = new P2PHandover
        {
            Id = Guid.NewGuid(),
            FinderId = command.FinderId,
            OwnerId = resolvedOwnerId,
            FinderPostId = command.FinderPostId,
            OwnerPostId = command.OwnerPostId,
            Status = HandoverStatus.Pending,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        await handoverRepository.CreateAsync(handover);
        await handoverRepository.SaveChangesAsync();

        return new HandoverResult
        {
            Id = handover.Id,
            Type = "P2P",
            FinderPostId = handover.FinderPostId,
            OwnerPostId = handover.OwnerPostId,
            Status = handover.Status.ToString(),
            ConfirmedAt = handover.ConfirmedAt,
            ExpiresAt = handover.ExpiresAt,
            CreatedAt = handover.CreatedAt
        };
    }
}
