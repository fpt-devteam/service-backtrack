using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostMatchings;
using Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostEmbedding;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Backtrack.Core.Application.Usecases.Posts.CreatePost;

public sealed class CreatePostHandler(
    IPostRepository postRepository,
    IOrganizationRepository organizationRepository,
    IMembershipRepository membershipRepository,
    IOrgReceiveReportRepository receiveReportRepository,
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
            Item = command.Item,
            Location = location!,
            ExternalPlaceId = externalPlaceId,
            DisplayAddress = displayAddress!,
            Embedding = null,
            EmbeddingStatus = EmbeddingStatus.Pending,
            PostMatchingStatus = PostMatchingStatus.Pending,
            ContentHash = hasher.HashStrings(JsonSerializer.Serialize(command.Item)),
            EventTime = command.EventTime ?? DateTimeOffset.UtcNow,
            ImageUrls = command.ImageUrls.ToList(),
            CreatedAt = DateTimeOffset.UtcNow
        };

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

        backgroundJobService.EnqueueJob<PostEmbeddingOrchestrator>(orchestrator => orchestrator.GenerateEmbeddingAndFindMatchesAsync(post.Id));

        return new PostResult
        {
            Id = post.Id,
            Organization = post.Organization?.ToOrganizationOnPost(),
            PostType = post.PostType,
            Status = post.Status,
            Item = post.Item,
            ImageUrls = post.ImageUrls,
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        };
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
