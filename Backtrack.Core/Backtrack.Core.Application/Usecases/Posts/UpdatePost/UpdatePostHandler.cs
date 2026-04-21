using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostEmbedding;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Backtrack.Core.Application.Usecases.PostMatchings;

namespace Backtrack.Core.Application.Usecases.Posts.UpdatePost;

public sealed class UpdatePostHandler : IRequestHandler<UpdatePostCommand, PostResult>
{
    private readonly IPostRepository _postRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly IHasher _hasher;

    public UpdatePostHandler(
        IPostRepository postRepository,
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository,
        IBackgroundJobService backgroundJobService,
        IHasher hasher)
    {
        _postRepository = postRepository;
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
        _backgroundJobService = backgroundJobService;
        _hasher = hasher;
    }

    public async Task<PostResult> Handle(UpdatePostCommand command, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(command.PostId, true);
        if (post == null) throw new NotFoundException(PostErrors.NotFound);

        if (post.OrganizationId.HasValue)
        {
            var membership = await _membershipRepository.GetByOrgAndUserAsync(post.OrganizationId.Value, command.UserId);
            if (membership is null) throw new ForbiddenException(PostErrors.Forbidden);
        }
        else
        {
            if (post.AuthorId != command.UserId) throw new ForbiddenException(PostErrors.Forbidden);
        }

        bool needsReEmbedding = false;

        if (command.PostType != null && !Enum.TryParse<PostType>(command.PostType, ignoreCase: true, out var postType))
        {
            throw new ValidationException(PostErrors.InvalidPostType);
        }

        if (command.OrganizationId.HasValue && post.OrganizationId != command.OrganizationId)
        {
            var organization = await _organizationRepository.GetByIdAsync(command.OrganizationId.Value)
                ?? throw new NotFoundException(OrganizationErrors.NotFound);

            if (await _membershipRepository.GetByOrgAndUserAsync(organization.Id, command.UserId) == null)
                throw new ValidationException(MembershipErrors.MemberNotFound);

            if (post.PostType == PostType.Lost) throw new ValidationException(PostErrors.LostPostCannotBeAssociatedWithOrganization);

            post.OrganizationId = command.OrganizationId;
            post.Organization = organization;
            post.Location = organization.Location;
            post.DisplayAddress = organization.DisplayAddress;
            post.ExternalPlaceId = organization.ExternalPlaceId;
            needsReEmbedding = true;
        }
        else if (command.OrganizationId == null && post.OrganizationId != null)
        {
            post.OrganizationId = null;
            post.Organization = null;
            needsReEmbedding = true;
        }

        // Update category-specific detail if provided
        if (command.PersonalBelongingDetail != null)
        {
            UpdatePersonalBelongingDetail(post, command.PersonalBelongingDetail);
            post.PostTitle = post.PersonalBelongingDetail?.ItemName ?? post.PostTitle;
            needsReEmbedding = true;
        }
        else if (command.CardDetail != null)
        {
            UpdateCardDetail(post, command.CardDetail, _hasher);
            post.PostTitle = post.CardDetail?.ItemName ?? post.PostTitle;
            needsReEmbedding = true;
        }
        else if (command.ElectronicDetail != null)
        {
            UpdateElectronicDetail(post, command.ElectronicDetail);
            post.PostTitle = post.ElectronicDetail?.ItemName ?? post.PostTitle;
            needsReEmbedding = true;
        }
        else if (command.OtherDetail != null)
        {
            UpdateOtherDetail(post, command.OtherDetail);
            post.PostTitle = post.OtherDetail?.ItemName ?? post.PostTitle;
            needsReEmbedding = true;
        }

        if (command.Location != null)
        {
            var newLocation = new GeoPoint(command.Location.Latitude, command.Location.Longitude);
            if (post.Location == null ||
                DoubleUtil.AreNotApproximatelyEqual(post.Location.Latitude, newLocation.Latitude) ||
                DoubleUtil.AreNotApproximatelyEqual(post.Location.Longitude, newLocation.Longitude))
            {
                post.Location = newLocation;
                needsReEmbedding = true;
            }
        }

        post.ExternalPlaceId = command.ExternalPlaceId ?? post.ExternalPlaceId;
        post.DisplayAddress = command.DisplayAddress ?? post.DisplayAddress;

        if (command.ImageUrls != null)
        {
            post.ImageUrls = command.ImageUrls.ToList();
            needsReEmbedding = true;
        }

        post.EventTime = command.EventTime.HasValue ? command.EventTime.Value : post.EventTime;

        if (command.Status is not null && Enum.TryParse<PostStatus>(command.Status, ignoreCase: true, out var parsedStatus))
            post.Status = parsedStatus;

        post.UpdatedAt = DateTimeOffset.UtcNow;

        if (needsReEmbedding)
        {
            post.EmbeddingStatus = EmbeddingStatus.Pending;
            post.PostMatchingStatus = PostMatchingStatus.Pending;
        }
        await _postRepository.SaveChangesAsync();

        if (needsReEmbedding)
            _backgroundJobService.EnqueueJob<PostEmbeddingOrchestrator>(
                orchestrator => orchestrator.GenerateEmbeddingAndFindMatchesAsync(post.Id));

        return post.ToPostResult();
    }

    private static void UpdatePersonalBelongingDetail(Post post, PersonalBelongingDetailInput input)
    {
        if (post.PersonalBelongingDetail is { } d)
        {
            d.ItemName = input.ItemName ?? d.ItemName;
            d.Color = input.Color ?? d.Color;
            d.Brand = input.Brand ?? d.Brand;
            d.Material = input.Material ?? d.Material;
            d.Size = input.Size ?? d.Size;
            d.Condition = input.Condition ?? d.Condition;
            d.DistinctiveMarks = input.DistinctiveMarks ?? d.DistinctiveMarks;
            d.AdditionalDetails = input.AdditionalDetails ?? d.AdditionalDetails;
        }
        else
        {
            post.PersonalBelongingDetail = new PostPersonalBelongingDetail
            {
                PostId = post.Id,
                ItemName = input.ItemName,
                Color = input.Color,
                Brand = input.Brand,
                Material = input.Material,
                Size = input.Size,
                Condition = input.Condition,
                DistinctiveMarks = input.DistinctiveMarks,
                AdditionalDetails = input.AdditionalDetails
            };
        }
    }

    private static void UpdateCardDetail(Post post, CardDetailInput input, IHasher hasher)
    {
        var newHash   = input.CardNumber is not null ? hasher.Hash(input.CardNumber) : null;
        var newMasked = MaskCardNumber(input.CardNumber);

        if (post.CardDetail is { } d)
        {
            if (newHash is not null)   { d.CardNumberHash   = newHash;   d.CardNumberMasked = newMasked; }
            d.ItemName             = input.ItemName             ?? d.ItemName;
            d.HolderName           = input.HolderName           ?? d.HolderName;
            d.HolderNameNormalized = input.HolderNameNormalized ?? d.HolderNameNormalized;
            d.DateOfBirth          = input.DateOfBirth          ?? d.DateOfBirth;
            d.IssueDate            = input.IssueDate            ?? d.IssueDate;
            d.ExpiryDate           = input.ExpiryDate           ?? d.ExpiryDate;
            d.IssuingAuthority     = input.IssuingAuthority     ?? d.IssuingAuthority;
            d.OcrText              = input.OcrText              ?? d.OcrText;
            d.AdditionalDetails    = input.AdditionalDetails    ?? d.AdditionalDetails;
        }
        else
        {
            post.CardDetail = new PostCardDetail
            {
                PostId               = post.Id,
                ItemName             = input.ItemName,
                CardNumberHash       = newHash,
                CardNumberMasked     = newMasked,
                HolderName           = input.HolderName,
                HolderNameNormalized = input.HolderNameNormalized,
                DateOfBirth          = input.DateOfBirth,
                IssueDate            = input.IssueDate,
                ExpiryDate           = input.ExpiryDate,
                IssuingAuthority     = input.IssuingAuthority,
                OcrText              = input.OcrText,
                AdditionalDetails    = input.AdditionalDetails
            };
        }
    }

    private static string? MaskCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber)) return null;
        var digits = cardNumber.Replace("-", "").Replace(" ", "");
        var last4 = digits.Length >= 4 ? digits[^4..] : digits;
        return $"***{last4}";
    }

    private static void UpdateElectronicDetail(Post post, ElectronicDetailInput input)
    {
        if (post.ElectronicDetail is { } d)
        {
            d.ItemName = input.ItemName ?? d.ItemName;
            d.Brand = input.Brand ?? d.Brand;
            d.Model = input.Model ?? d.Model;
            d.Color = input.Color ?? d.Color;
            d.HasCase = input.HasCase ?? d.HasCase;
            d.CaseDescription = input.CaseDescription ?? d.CaseDescription;
            d.ScreenCondition = input.ScreenCondition ?? d.ScreenCondition;
            d.LockScreenDescription = input.LockScreenDescription ?? d.LockScreenDescription;
            d.DistinguishingFeatures = input.DistinguishingFeatures ?? d.DistinguishingFeatures;
            d.AdditionalDetails = input.AdditionalDetails ?? d.AdditionalDetails;
        }
        else
        {
            post.ElectronicDetail = new PostElectronicDetail
            {
                PostId = post.Id,
                ItemName = input.ItemName,
                Brand = input.Brand,
                Model = input.Model,
                Color = input.Color,
                HasCase = input.HasCase,
                CaseDescription = input.CaseDescription,
                ScreenCondition = input.ScreenCondition,
                LockScreenDescription = input.LockScreenDescription,
                DistinguishingFeatures = input.DistinguishingFeatures,
                AdditionalDetails = input.AdditionalDetails
            };
        }
    }

    private static void UpdateOtherDetail(Post post, OtherDetailInput input)
    {
        if (post.OtherDetail is { } d)
        {
            d.ItemName = input.ItemName;
            d.PrimaryColor = input.PrimaryColor ?? d.PrimaryColor;
            d.AdditionalDetails = input.AdditionalDetails ?? d.AdditionalDetails;
        }
        else
        {
            post.OtherDetail = new PostOtherDetail
            {
                PostId = post.Id,
                ItemName = input.ItemName,
                PrimaryColor = input.PrimaryColor,
                AdditionalDetails = input.AdditionalDetails
            };
        }
    }
}
