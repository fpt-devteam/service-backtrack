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
        await postRepository.SaveChangesAsync();

        // backgroundJobService.EnqueueJob<PostEmbeddingOrchestrator>(orchestrator => orchestrator.GenerateEmbeddingAndFindMatchesAsync(post.Id));

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
}
