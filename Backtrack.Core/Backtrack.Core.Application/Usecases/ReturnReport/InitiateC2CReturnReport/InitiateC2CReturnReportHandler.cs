using Backtrack.Core.Application.Events;
using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Messaging;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
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
        var finderPost = await postRepository.GetByIdAsync(command.FinderPostId)
            ?? throw new NotFoundException(ReturnReportErrors.FinderPostNotFound);

        if (finderPost.PostType != PostType.Found)
            throw new ValidationException(ReturnReportErrors.PostTypeMismatch);

        var ownerPost = await postRepository.GetByIdAsync(command.OwnerPostId)
            ?? throw new NotFoundException(ReturnReportErrors.OwnerPostNotFound);

        if (ownerPost.PostType != PostType.Lost)
            throw new ValidationException(ReturnReportErrors.PostTypeMismatch);

        // Roles are determined by post authorship
        var finderId = finderPost.AuthorId;
        var ownerId  = ownerPost.AuthorId;

        if (finderId == ownerId)
            throw new ValidationException(ReturnReportErrors.CannotReturnReportOwnPost);

        // Initiator must be one of the two participants
        if (command.InitiatorId != finderId && command.InitiatorId != ownerId)
            throw new ForbiddenException(ReturnReportErrors.NotParticipant);

        var finder = await userRepository.GetByIdAsync(finderId)
            ?? throw new NotFoundException(ReturnReportErrors.FinderNotFound);

        var owner = await userRepository.GetByIdAsync(ownerId)
            ?? throw new NotFoundException(ReturnReportErrors.OwnerNotFound);

        var existingReturnReport = await returnReportRepository.GetOngoingByParticipantsAndPostsAsync(
            finderId,
            ownerId,
            command.FinderPostId,
            command.OwnerPostId,
            cancellationToken);

        if (existingReturnReport is not null)
        {
            return new C2CReturnReportResult
            {
                Id              = existingReturnReport.Id,
                Finder          = finder.ToUserResult(),
                Owner           = owner.ToUserResult(),
                FinderPost      = finderPost.ToPostResult(),
                OwnerPost       = ownerPost.ToPostResult(),
                Status          = existingReturnReport.Status.ToString(),
                ActivatedByRole = null,
                ConfirmedAt     = existingReturnReport.ConfirmedAt,
                ExpiresAt       = existingReturnReport.ExpiresAt,
                CreatedAt       = existingReturnReport.CreatedAt
            };
        }

        var returnReport = new C2CReturnReport
        {
            Id           = Guid.NewGuid(),
            FinderId     = finderId,
            OwnerId      = ownerId,
            FinderPostId = command.FinderPostId,
            OwnerPostId  = command.OwnerPostId,
            Status       = C2CReturnReportStatus.Ongoing,
            ExpiresAt    = DateTimeOffset.UtcNow.AddDays(7)
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
            FinderPostType     = finderPost.PostType.ToString(),
            OwnerPostId        = returnReport.OwnerPostId,
            OwnerPostType      = ownerPost.PostType.ToString(),
            Status             = returnReport.Status.ToString(),
            ActivatedByRole    = null,
            ConfirmedAt        = returnReport.ConfirmedAt,
            ExpiresAt          = returnReport.ExpiresAt,
            CreatedAt          = returnReport.CreatedAt,
            EventTimestamp     = DateTimeOffset.UtcNow,
        });

        return new C2CReturnReportResult
        {
            Id              = returnReport.Id,
            Finder          = finder.ToUserResult(),
            Owner           = owner.ToUserResult(),
            FinderPost      = finderPost.ToPostResult(),
            OwnerPost       = ownerPost.ToPostResult(),
            Status          = returnReport.Status.ToString(),
            ActivatedByRole = null,
            ConfirmedAt     = returnReport.ConfirmedAt,
            ExpiresAt       = returnReport.ExpiresAt,
            CreatedAt       = returnReport.CreatedAt
        };
    }
}
