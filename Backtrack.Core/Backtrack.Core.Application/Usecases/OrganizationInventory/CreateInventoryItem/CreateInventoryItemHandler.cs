using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.OrganizationInventory.SearchInventoryItems;
using Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostEmbedding;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.OrganizationInventory.CreateInventoryItem;

public sealed class CreateInventoryItemHandler(
    IOrganizationRepository organizationRepository,
    IMembershipRepository membershipRepository,
    IPostRepository postRepository,
    IOrgReceiveReportRepository receiveReportRepository,
    ISubcategoryRepository subcategoryRepository,
    IHasher hasher,
    IBackgroundJobService backgroundJobService) : IRequestHandler<CreateInventoryItemCommand, InventoryItemResult>
{
    public async Task<InventoryItemResult> Handle(CreateInventoryItemCommand command, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(command.OrgId)
            ?? throw new NotFoundException(OrganizationErrors.NotFound);

        if (await membershipRepository.GetByOrgAndUserAsync(organization.Id, command.StaffId) == null)
            throw new ForbiddenException(MembershipErrors.NotAMember);

        if (!Enum.TryParse<ItemCategory>(command.Category, ignoreCase: true, out var category))
            throw new ValidationException(PostErrors.InvalidCategory);

        var subcategory = await subcategoryRepository.GetByCodeAsync(command.SubcategoryCode, cancellationToken)
            ?? throw new NotFoundException(PostErrors.SubcategoryNotFound);

        ValidateFinderInfo(command.FinderInfo, organization.RequiredFinderContractFields);

        var post = new Post
        {
            Id                 = Guid.NewGuid(),
            AuthorId           = command.StaffId,
            OrganizationId     = organization.Id,
            PostTitle          = command.PostTitle,
            PostType           = PostType.Found,
            Status             = PostStatus.InStorage,
            Category           = category,
            SubcategoryId      = subcategory.Id,
            Location           = organization.Location,
            InternalLocation   = command.InternalLocation,
            ExternalPlaceId    = organization.ExternalPlaceId,
            DisplayAddress     = organization.DisplayAddress,
            Embedding          = null,
            EmbeddingStatus    = EmbeddingStatus.Pending,
            PostMatchingStatus = PostMatchingStatus.Pending,
            EventTime          = command.EventTime,
            ImageUrls          = command.ImageUrls.ToList(),
        };

        AttachDetail(post, command, hasher);
        SetDetailContentHash(post, hasher);

        await postRepository.CreateAsync(post);

        var receiveReport = new OrgReceiveReport
        {
            Id         = Guid.NewGuid(),
            OrgId      = organization.Id,
            StaffId    = command.StaffId,
            PostId     = post.Id,
            FinderInfo = command.FinderInfo,
        };
        await receiveReportRepository.CreateAsync(receiveReport);

        await postRepository.SaveChangesAsync();

        backgroundJobService.EnqueueJob(new UpdatePostEmbeddingCommand(post.Id));

        return post.ToInventoryItemResult(receiveReport);
    }

    private static void AttachDetail(Post post, CreateInventoryItemCommand command, IHasher hasher)
    {
        if (command.PersonalBelongingDetail is { } pb)
            post.PersonalBelongingDetail = pb.ToEntity(post.Id);
        else if (command.CardDetail is { } cd)
            post.CardDetail = cd.ToEntity(post.Id, hasher);
        else if (command.ElectronicDetail is { } ed)
            post.ElectronicDetail = ed.ToEntity(post.Id);
        else if (command.OtherDetail is { } od)
            post.OtherDetail = od.ToEntity(post.Id);
    }

    private static void SetDetailContentHash(Post post, IHasher hasher)
    {
        if (post.Category == ItemCategory.Cards) return;
        var hash = hasher.HashStrings(PostDocumentUtil.BuildDocument(post));
        if (post.PersonalBelongingDetail is not null)      post.PersonalBelongingDetail.ContentHash = hash;
        else if (post.ElectronicDetail is not null)        post.ElectronicDetail.ContentHash = hash;
        else if (post.OtherDetail is not null)             post.OtherDetail.ContentHash = hash;
    }

    private static void ValidateFinderInfo(FinderInfo? finderInfo, List<OrgContractField> requiredFields)
    {
        if (requiredFields.Count == 0) return;

        if (finderInfo is null)
            throw new ValidationException(new Error(
                "FinderInfoRequired",
                "FinderInfo is required for organization inventory items."));

        var missingFields = requiredFields
            .Where(field => field switch
            {
                OrgContractField.Email       => string.IsNullOrWhiteSpace(finderInfo.Email),
                OrgContractField.Phone       => string.IsNullOrWhiteSpace(finderInfo.Phone),
                OrgContractField.NationalId  => string.IsNullOrWhiteSpace(finderInfo.NationalId),
                OrgContractField.OrgMemberId => string.IsNullOrWhiteSpace(finderInfo.OrgMemberId),
                OrgContractField.FullName    => string.IsNullOrWhiteSpace(finderInfo.FinderName),
                _                            => false
            })
            .Select(f => f.ToString())
            .ToList();

        if (missingFields.Count > 0)
            throw new ValidationException(new Error(
                "FinderInfoMissingRequiredFields",
                $"FinderInfo is missing required fields: {string.Join(", ", missingFields)}."));
    }
}
