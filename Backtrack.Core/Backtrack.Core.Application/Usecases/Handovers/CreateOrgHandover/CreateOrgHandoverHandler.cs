using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Handovers.CreateOrgHandover;

public sealed class CreateOrgHandoverHandler(
    IHandoverRepository handoverRepository,
    IPostRepository postRepository,
    IMembershipRepository membershipRepository,
    IOrganizationRepository organizationRepository) : IRequestHandler<CreateOrgHandoverCommand, HandoverResult>
{
    public async Task<HandoverResult> Handle(CreateOrgHandoverCommand command, CancellationToken cancellationToken)
    {
        // Validate user is staff of the organization
        var membership = await membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken);
        if (membership == null)
        {
            throw new ForbiddenException(MembershipErrors.NotAMember);
        }

        // Validate organization exists
        var org = await organizationRepository.GetByIdAsync(command.OrgId)
            ?? throw new NotFoundException(OrganizationErrors.NotFound);

        // Validate finder post exists and belongs to the organization
        var finderPost = await postRepository.GetByIdAsync(command.PostId)
            ?? throw new NotFoundException(HandoverErrors.FinderPostNotFound);

        if (finderPost.OrganizationId != command.OrgId)
        {
            throw new ForbiddenException(new Error("PostNotInOrg", "This post does not belong to your organization."));
        }

        if (finderPost.PostType != PostType.Found)
        {
            throw new ValidationException(HandoverErrors.PostTypeMismatch);
        }

        // Check if active handover already exists
        var exists = await handoverRepository.ExistsActiveHandoverForPostsAsync(
            command.PostId, null, cancellationToken);
        if (exists)
        {
            throw new ConflictException(HandoverErrors.AlreadyExists);
        }

        var handover = new OrgHandover
        {
            Id = Guid.NewGuid(),
            FinderId = finderPost.AuthorId,
            OrgId = command.OrgId,
            StaffId = command.UserId,
            FinderPostId = command.PostId,
            OwnerVerified = false,
            Status = HandoverStatus.Pending,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };

        await handoverRepository.CreateAsync(handover);
        await handoverRepository.SaveChangesAsync();

        return new HandoverResult
        {
            Id = handover.Id,
            Type = "Org",
            FinderPostId = handover.FinderPostId,
            OwnerPostId = null,
            Status = handover.Status.ToString(),
            ConfirmedAt = handover.ConfirmedAt,
            ExpiresAt = handover.ExpiresAt,
            CreatedAt = handover.CreatedAt,
            OrgExtension = new HandoverOrgExtensionResult
            {
                Id = handover.Id,
                OrgId = handover.OrgId,
                StaffId = handover.StaffId,
                OwnerVerified = handover.OwnerVerified,
                OwnerFormData = handover.OwnerFormData,
                StaffConfirmedAt = handover.StaffConfirmedAt,
                OwnerConfirmedAt = handover.OwnerConfirmedAt
            }
        };
    }
}
