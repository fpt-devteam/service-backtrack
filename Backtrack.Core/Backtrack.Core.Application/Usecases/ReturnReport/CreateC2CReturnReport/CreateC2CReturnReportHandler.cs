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
        string resolvedOwnerId;
        Post? finderPost = null;
        Post? ownerPost = null;
        User? owner = null;

        if (command.FinderPostId.HasValue)
        {
            finderPost = await postRepository.GetByIdAsync(command.FinderPostId.Value)
                ?? throw new NotFoundException(ReturnReportErrors.FinderPostNotFound);

            if (finderPost.AuthorId != command.FinderId)
            {
                throw new ForbiddenException(new Error("NotPostOwner", "You can only create returnReports for your own posts."));
            }

            if (finderPost.PostType != PostType.Found)
            {
                throw new ValidationException(ReturnReportErrors.PostTypeMismatch);
            }
        }

        // Validate owner post if provided (Case 1, 2)
        if (command.OwnerPostId.HasValue)
        {
            ownerPost = await postRepository.GetByIdAsync(command.OwnerPostId.Value)
                ?? throw new NotFoundException(ReturnReportErrors.OwnerPostNotFound);

            if (ownerPost.PostType != PostType.Lost)
            {
                throw new ValidationException(ReturnReportErrors.PostTypeMismatch);
            }

            if (ownerPost.AuthorId == command.FinderId)
            {
                throw new ValidationException(ReturnReportErrors.CannotReturnReportOwnPost);
            }
            resolvedOwnerId = ownerPost.AuthorId;
        }
        else
        {
            // OwnerPostId is null (Case 3, 4) - OwnerId is required
            if (string.IsNullOrWhiteSpace(command.OwnerId))
            {
                throw new ValidationException(ReturnReportErrors.OwnerIdRequired);
            }

            if (command.OwnerId == command.FinderId)
            {
                throw new ValidationException(ReturnReportErrors.CannotReturnReportOwnPost);
            }

            owner = await userRepository.GetByIdAsync(command.OwnerId) ?? throw new NotFoundException(ReturnReportErrors.OwnerNotFound);
            resolvedOwnerId = command.OwnerId;
        }

        var finder = await userRepository.GetByIdAsync(command.FinderId)
            ?? throw new NotFoundException(UserErrors.NotFound);

        owner ??= await userRepository.GetByIdAsync(resolvedOwnerId)
            ?? throw new NotFoundException(ReturnReportErrors.OwnerNotFound);

        if (command.FinderPostId.HasValue && command.OwnerPostId.HasValue)
        {
            // Check if active returnReport already exists for this finder-owner pair
            var exists = await returnReportRepository.ExistsActiveReturnReportForPostsAsync(
                command.FinderPostId.Value, command.OwnerPostId.Value, cancellationToken);
            if (exists)
            {
                throw new ConflictException(ReturnReportErrors.AlreadyExists);
            }
        }
       

        var returnReport = new C2CReturnReport
        {
            Id = Guid.NewGuid(),
            FinderId = command.FinderId,
            OwnerId = resolvedOwnerId,
            FinderPostId = command.FinderPostId,
            OwnerPostId = command.OwnerPostId,
            Status = ReturnReportStatus.Pending,
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
}
