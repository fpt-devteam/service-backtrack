using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
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
    IUserRepository userRepository) : IRequestHandler<InitiateC2CReturnReportCommand, C2CReturnReportResult>
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

        var finder = await userRepository.GetByIdAsync(finderPost.AuthorId)
            ?? throw new NotFoundException(ReturnReportErrors.FinderNotFound);

        var owner = await userRepository.GetByIdAsync(ownerPost.AuthorId)
            ?? throw new NotFoundException(ReturnReportErrors.OwnerNotFound);

        // Roles are determined by post authorship
        if (finder.Id == owner.Id)
            throw new ValidationException(ReturnReportErrors.CannotReturnReportOwnPost);

        // Initiator must be one of the two participants
        if (command.InitiatorId != finder.Id && command.InitiatorId != owner.Id)
            throw new ForbiddenException(ReturnReportErrors.NotParticipant);


        var existingReturnReport = await returnReportRepository.GetOngoingByParticipantsAndPostsAsync(
            finder.Id,
            owner.Id,
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
                Status          = existingReturnReport.Status,
                ConfirmedAt     = existingReturnReport.ConfirmedAt,
                ExpiresAt       = existingReturnReport.ExpiresAt,
                CreatedAt       = existingReturnReport.CreatedAt
            };
        }

        var returnReport = new C2CReturnReport
        {
            Id           = Guid.NewGuid(),
            FinderId     = finder.Id,
            OwnerId      = owner.Id,
            FinderPostId = command.FinderPostId,
            OwnerPostId  = command.OwnerPostId,
            Status       = C2CReturnReportStatus.Ongoing,
            ExpiresAt    = DateTimeOffset.UtcNow.AddDays(7)
        };

        await returnReportRepository.CreateAsync(returnReport);
        await returnReportRepository.SaveChangesAsync();

        return returnReport.ToC2CReturnReportResult();
    }
}
