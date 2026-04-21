using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostMatchings;
using Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostEmbedding;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.Posts.CreatePost;

public sealed class CreatePostHandler(
    IPostRepository postRepository,
    IOrganizationRepository organizationRepository,
    IMembershipRepository membershipRepository,
    IOrgReceiveReportRepository receiveReportRepository,
    ISubcategoryRepository subcategoryRepository,
    IHasher hasher,
    IBackgroundJobService backgroundJobService,
    ILogger<CreatePostHandler> logger) : IRequestHandler<CreatePostCommand, PostResult>
{
    public async Task<PostResult> Handle(CreatePostCommand command, CancellationToken cancellationToken)
    {
        PostType postType;
        GeoPoint? location = command.Location;
        string? displayAddress = command.DisplayAddress;
        string? externalPlaceId = command.ExternalPlaceId;
        var status = PostStatus.Active;

        if (!Enum.TryParse<ItemCategory>(command.Category, ignoreCase: true, out var category))
            throw new ValidationException(PostErrors.InvalidCategory);

        var subcategory = await subcategoryRepository.GetByCodeAsync(command.SubcategoryCode, cancellationToken)
            ?? throw new NotFoundException(PostErrors.SubcategoryNotFound);

        if (command.OrganizationId.HasValue)
        {
            var organization = await organizationRepository.GetByIdAsync(command.OrganizationId.Value)
                ?? throw new NotFoundException(OrganizationErrors.NotFound);

            if (await membershipRepository.GetByOrgAndUserAsync(organization.Id, command.AuthorId) == null)
                throw new ForbiddenException(MembershipErrors.NotAMember);

            ValidateFinderInfo(command.FinderInfo, organization.RequiredFinderContractFields);

            postType = PostType.Found;
            status = PostStatus.InStorage;
            location = organization.Location;
            displayAddress = command.DisplayAddress ?? organization.DisplayAddress;
            externalPlaceId = organization.ExternalPlaceId;
        }
        else
        {
            if (!Enum.TryParse<PostType>(command.PostType, ignoreCase: true, out postType))
                throw new ValidationException(PostErrors.InvalidPostType);
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = command.AuthorId,
            OrganizationId = command.OrganizationId,
            PostType = postType,
            Status = status,
            Category = category,
            SubcategoryId = subcategory.Id,
            Location = location!,
            ExternalPlaceId = externalPlaceId,
            DisplayAddress = displayAddress!,
            Embedding = null,
            EmbeddingStatus = EmbeddingStatus.Pending,
            PostMatchingStatus = PostMatchingStatus.Pending,
            EventTime = command.EventTime ?? DateTimeOffset.UtcNow,
            ImageUrls = command.ImageUrls.ToList(),
            PostTitle = command.PostTitle,
            CreatedAt = DateTimeOffset.UtcNow
        };

        AttachDetail(post, command, hasher);
        SetDetailContentHash(post, hasher);

        await postRepository.CreateAsync(post);

        if (command.OrganizationId.HasValue)
        {
            var receiveReport = new OrgReceiveReport
            {
                Id = Guid.NewGuid(),
                OrgId = command.OrganizationId.Value,
                StaffId = command.AuthorId,
                PostId = post.Id,
                FinderInfo = command.FinderInfo,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await receiveReportRepository.CreateAsync(receiveReport);
        }

        await postRepository.SaveChangesAsync();

        if (!command.OrganizationId.HasValue)
            backgroundJobService.EnqueueJob<PostEmbeddingOrchestrator>(
                orchestrator => orchestrator.GenerateEmbeddingAndFindMatchesAsync(post.Id));

        return post.ToPostResult();
    }

    private static void SetDetailContentHash(Post post, IHasher hasher)
    {
        if (post.Category == ItemCategory.Cards) return;

        var hash = hasher.HashStrings(PostDocumentUtil.BuildDocument(post));

        if (post.PersonalBelongingDetail is not null) post.PersonalBelongingDetail.ContentHash = hash;
        else if (post.ElectronicDetail is not null) post.ElectronicDetail.ContentHash = hash;
        else if (post.OtherDetail is not null) post.OtherDetail.ContentHash = hash;
    }

    private static string? MaskCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber)) return null;
        var digits = cardNumber.Replace("-", "").Replace(" ", "");
        var last4 = digits.Length >= 4 ? digits[^4..] : digits;
        return $"***{last4}";
    }

    private static void AttachDetail(Post post, CreatePostCommand command, IHasher hasher)
    {
        if (command.PersonalBelongingDetail is { } pb)
        {
            post.PersonalBelongingDetail = new PostPersonalBelongingDetail
            {
                PostId = post.Id,
                ItemName = pb.ItemName,
                Color = pb.Color,
                Brand = pb.Brand,
                Material = pb.Material,
                Size = pb.Size,
                Condition = pb.Condition,
                DistinctiveMarks = pb.DistinctiveMarks,
                AdditionalDetails = pb.AdditionalDetails
            };
        }
        else if (command.CardDetail is { } cd)
        {
            post.CardDetail = new PostCardDetail
            {
                PostId               = post.Id,
                ItemName             = cd.ItemName,
                CardNumberHash       = cd.CardNumber is not null ? hasher.Hash(cd.CardNumber) : null,
                CardNumberMasked     = MaskCardNumber(cd.CardNumber),
                HolderName           = cd.HolderName,
                HolderNameNormalized = cd.HolderNameNormalized,
                DateOfBirth          = cd.DateOfBirth,
                IssueDate            = cd.IssueDate,
                ExpiryDate           = cd.ExpiryDate,
                IssuingAuthority     = cd.IssuingAuthority,
                OcrText              = cd.OcrText,
                AdditionalDetails    = cd.AdditionalDetails
            };
        }
        else if (command.ElectronicDetail is { } ed)
        {
            post.ElectronicDetail = new PostElectronicDetail
            {
                PostId = post.Id,
                ItemName = ed.ItemName,
                Brand = ed.Brand,
                Model = ed.Model,
                Color = ed.Color,
                HasCase = ed.HasCase,
                CaseDescription = ed.CaseDescription,
                ScreenCondition = ed.ScreenCondition,
                LockScreenDescription = ed.LockScreenDescription,
                DistinguishingFeatures = ed.DistinguishingFeatures,
                AdditionalDetails = ed.AdditionalDetails
            };
        }
        else if (command.OtherDetail is { } od)
        {
            post.OtherDetail = new PostOtherDetail
            {
                PostId = post.Id,
                ItemName = od.ItemName,
                PrimaryColor = od.PrimaryColor,
                AdditionalDetails = od.AdditionalDetails
            };
        }
    }

    private static void ValidateFinderInfo(FinderInfo? finderInfo, List<OrgContractField> requiredFields)
    {
        if (requiredFields.Count == 0)
            return;

        if (finderInfo is null)
            throw new ValidationException(new Error(
                "FinderInfoRequired",
                "FinderInfo is required for organization inventory items."));

        var missingFields = new List<string>();

        foreach (var field in requiredFields)
        {
            var isEmpty = field switch
            {
                OrgContractField.Email => string.IsNullOrWhiteSpace(finderInfo.Email),
                OrgContractField.Phone => string.IsNullOrWhiteSpace(finderInfo.Phone),
                OrgContractField.NationalId => string.IsNullOrWhiteSpace(finderInfo.NationalId),
                OrgContractField.OrgMemberId => string.IsNullOrWhiteSpace(finderInfo.OrgMemberId),
                OrgContractField.FullName => string.IsNullOrWhiteSpace(finderInfo.FinderName),
                _ => false
            };

            if (isEmpty)
                missingFields.Add(field.ToString());
        }

        if (missingFields.Count > 0)
            throw new ValidationException(new Error(
                "FinderInfoMissingRequiredFields",
                $"FinderInfo is missing required fields: {string.Join(", ", missingFields)}."));
    }
}
