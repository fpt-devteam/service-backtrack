using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.ArchiveInventoryItem;

public sealed class ArchiveInventoryItemHandler(
    IPostRepository postRepository,
    IMembershipRepository membershipRepository) : IRequestHandler<ArchiveInventoryItemCommand>
{
    public async Task<Unit> Handle(ArchiveInventoryItemCommand command, CancellationToken cancellationToken)
    {
        var membership = await membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken);
        if (membership is null) throw new ForbiddenException(MembershipErrors.NotAMember);

        var post = await postRepository.GetByIdAsync(command.PostId, isTrack: true)
            ?? throw new NotFoundException(PostErrors.NotFound);

        if (post.OrganizationId != command.OrgId)
            throw new ForbiddenException(PostErrors.Forbidden);

        if (post.Status != PostStatus.InStorage)
            throw new ConflictException(PostErrors.NotInStorage);

        post.Status = PostStatus.Archived;
        postRepository.Update(post);
        await postRepository.SaveChangesAsync();

        return Unit.Value;
    }
}
