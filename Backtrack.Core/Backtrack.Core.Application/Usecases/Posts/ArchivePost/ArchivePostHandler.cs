using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.ArchivePost;

public sealed class ArchivePostHandler(
    IPostRepository postRepository,
    IPostMatchRepository postMatchRepository,
    IC2CReturnReportRepository returnReportRepository) : IRequestHandler<ArchivePostCommand>
{
    public async Task<Unit> Handle(ArchivePostCommand command, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(command.PostId, true)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (post.OrganizationId.HasValue || post.AuthorId != command.UserId) throw new ForbiddenException(PostErrors.Forbidden);

        if (post.Status != PostStatus.Active)
            throw new ConflictException(PostErrors.CannotDelete);

        var openReports = await returnReportRepository.GetOpenByPostIdAsync(post.Id, cancellationToken);
        if (openReports.Count > 0)
        {
            foreach (var report in openReports)
                report.Status = C2CReturnReportStatus.Closed;
            await returnReportRepository.SaveChangesAsync();
        }

        await postMatchRepository.DeleteByPostIdAsync(post.Id, cancellationToken);

        post.Status = PostStatus.Archived;
        post.ArchivedAt = DateTimeOffset.UtcNow;
        postRepository.Update(post);
        await postRepository.SaveChangesAsync();

        return Unit.Value;
    }
}
