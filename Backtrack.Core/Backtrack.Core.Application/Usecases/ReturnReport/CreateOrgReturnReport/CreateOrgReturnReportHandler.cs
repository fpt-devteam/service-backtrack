using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.CreateOrgReturnReport;

public sealed class CreateOrgReturnReportHandler(
    IOrgReturnReportRepository orgReturnReportRepository,
    IPostRepository postRepository,
    IOrganizationRepository organizationRepository,
    IMembershipRepository membershipRepository,
    IUserRepository userRepository) : IRequestHandler<CreateOrgReturnReportCommand, OrgReturnReportResult>
{
    public async Task<OrgReturnReportResult> Handle(CreateOrgReturnReportCommand command, CancellationToken cancellationToken)
    {
        if (await membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken) == null)
            throw new ForbiddenException(MembershipErrors.NotAMember);

        var org = await organizationRepository.GetByIdAsync(command.OrgId)
            ?? throw new NotFoundException(OrganizationErrors.NotFound);

        var post = await postRepository.GetByIdAsync(command.PostId, isTrack: true)
            ?? throw new NotFoundException(ReturnReportErrors.FinderPostNotFound);

        if (post.OrganizationId != command.OrgId)
            throw new ForbiddenException(ReturnReportErrors.PostNotInOrg);

        if (post.PostType != PostType.Found)
            throw new ValidationException(ReturnReportErrors.PostTypeMismatch);

        if (await orgReturnReportRepository.ExistsActiveForPostAsync(command.PostId, cancellationToken))
            throw new ConflictException(ReturnReportErrors.AlreadyExists);

        var staff = await userRepository.GetByIdAsync(command.UserId)
            ?? throw new NotFoundException(UserErrors.NotFound);

        var returnReport = new OrgReturnReport
        {
            Id = Guid.NewGuid(),
            OrgId = command.OrgId,
            StaffId = command.UserId,
            PostId = command.PostId,
            EvidenceImageUrls = command.EvidenceImageUrls,
            OwnerInfo = command.OwnerInfo,
        };

        post.Status = PostStatus.Returned;
        postRepository.Update(post);

        await orgReturnReportRepository.CreateAsync(returnReport);
        await orgReturnReportRepository.SaveChangesAsync();

        returnReport.Organization = org;
        returnReport.Staff = staff;
        returnReport.Post = post;
        return returnReport.ToOrgReturnReportResult();
    }
}
