using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Organizations;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
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
        var membership = await membershipRepository.GetByOrgAndUserAsync(command.OrgId, command.UserId, cancellationToken)
            ?? throw new ForbiddenException(MembershipErrors.NotAMember);

        var org = await organizationRepository.GetByIdAsync(command.OrgId)
            ?? throw new NotFoundException(OrganizationErrors.NotFound);

        var post = await postRepository.GetByIdAsync(command.PostId, isTrack: true)
            ?? throw new NotFoundException(ReturnReportErrors.FinderPostNotFound);

        if (post.OrganizationId != command.OrgId)
        {
            throw new ForbiddenException(new Error("PostNotInOrg", "This post does not belong to your organization."));
        }

        if (post.PostType != PostType.Found)
        {
            throw new ValidationException(ReturnReportErrors.PostTypeMismatch);
        }

        var exists = await orgReturnReportRepository.ExistsActiveForPostAsync(command.PostId, cancellationToken);
        if (exists)
        {
            throw new ConflictException(ReturnReportErrors.AlreadyExists);
        }

        var staff = await userRepository.GetByIdAsync(command.UserId)
            ?? throw new NotFoundException(UserErrors.NotFound);

        var returnReport = new OrgReturnReport
        {
            Id = Guid.NewGuid(),
            OrgId = command.OrgId,
            StaffId = command.UserId,
            PostId = command.PostId,
            OwnerInfo = command.OwnerInfo,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };

        post.Status = PostStatus.Returned;
        postRepository.Update(post);

        await orgReturnReportRepository.CreateAsync(returnReport);
        await orgReturnReportRepository.SaveChangesAsync();

        return new OrgReturnReportResult
        {
            Id = returnReport.Id,
            Organization = new OrganizationResult
            {
                Id = org.Id,
                Name = org.Name,
                Slug = org.Slug,
                Location = org.Location,
                DisplayAddress = org.DisplayAddress,
                ExternalPlaceId = org.ExternalPlaceId,
                Phone = org.Phone,
                ContactEmail = org.ContactEmail,
                IndustryType = org.IndustryType,
                TaxIdentificationNumber = org.TaxIdentificationNumber,
                LogoUrl = org.LogoUrl,
                CoverImageUrl = org.CoverImageUrl,
                LocationNote = org.LocationNote,
                BusinessHours = org.BusinessHours,
                RequiredFinderContractFields = org.RequiredFinderContractFields,
                RequiredOwnerContractFields = org.RequiredOwnerContractFields,
                Status = org.Status.ToString(),
                CreatedAt = org.CreatedAt,
            },
            Staff = staff.ToUserResult(),
            OwnerInfo = returnReport.OwnerInfo,
            Post = post.ToPostResult(),
            ExpiresAt = returnReport.ExpiresAt,
            CreatedAt = returnReport.CreatedAt
        };
    }
}
