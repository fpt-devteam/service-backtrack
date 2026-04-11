using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.ActiveC2CReturnReport;

public sealed class ActiveC2CReturnReportHandler(
    IC2CReturnReportRepository returnReportRepository) : IRequestHandler<ActiveC2CReturnReportCommand, C2CReturnReportResult>
{
    public async Task<C2CReturnReportResult> Handle(ActiveC2CReturnReportCommand command, CancellationToken cancellationToken)
    {
        var returnReport = await returnReportRepository.GetByIdWithPostsAsync(command.C2CReturnReportId, cancellationToken)
            ?? throw new NotFoundException(ReturnReportErrors.NotFound);

        if (returnReport.Status != ReturnReportStatus.Draft)
            throw new ValidationException(new Error("NotDraft", "Only a Draft return report can be activated."));

        var isParticipant = returnReport.FinderId == command.UserId || returnReport.OwnerId == command.UserId;
        if (!isParticipant)
            throw new ForbiddenException(ReturnReportErrors.NotParticipant);

        // Check that activating this post won't conflict with an existing Active report for the same post
        if (returnReport.FinderId == command.UserId && returnReport.FinderPostId.HasValue)
        {
            var taken = await returnReportRepository.ExistsActiveReturnReportForFinderPostAsync(returnReport.FinderPostId.Value, cancellationToken);
            if (taken)
                throw new ConflictException(ReturnReportErrors.FinderPostAlreadyInReport);
        }

        if (returnReport.OwnerId == command.UserId && returnReport.OwnerPostId.HasValue)
        {
            var taken = await returnReportRepository.ExistsActiveReturnReportForOwnerPostAsync(returnReport.OwnerPostId.Value, cancellationToken);
            if (taken)
                throw new ConflictException(ReturnReportErrors.OwnerPostAlreadyInReport);
        }

        returnReport.Status = ReturnReportStatus.Active;
        returnReport.ActivatedById = command.UserId;

        await returnReportRepository.SaveChangesAsync();

        var finder = returnReport.FinderPost?.Author ?? returnReport.Finder!;
        var owner = returnReport.OwnerPost?.Author ?? returnReport.Owner!;

        return new C2CReturnReportResult
        {
            Id = returnReport.Id,
            Finder = finder.ToUserResult(),
            Owner = owner.ToUserResult(),
            FinderPost = returnReport.FinderPost?.ToPostResult(),
            OwnerPost = returnReport.OwnerPost?.ToPostResult(),
            Status = returnReport.Status.ToString(),
            ConfirmedAt = returnReport.ConfirmedAt,
            ExpiresAt = returnReport.ExpiresAt,
            CreatedAt = returnReport.CreatedAt
        };
    }
}
