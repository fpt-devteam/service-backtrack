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
        if (!Enum.TryParse<PostType>(command.PostType, ignoreCase: true, out var postType))
        {
            throw new ValidationException(PostErrors.InvalidPostType);
        }

        GeoPoint location = command.Location;
        string displayAddress = command.DisplayAddress;
        string? externalPlaceId = command.ExternalPlaceId;

        if (command.OrganizationId.HasValue)
        {
            var organization = await organizationRepository.GetByIdAsync(command.OrganizationId.Value)
                ?? throw new NotFoundException(OrganizationErrors.NotFound);

            if (await membershipRepository.GetByOrgAndUserAsync(organization.Id, command.AuthorId) == null)
            {
                throw new ValidationException(MembershipErrors.MemberNotFound);
            }

            if (postType == PostType.Lost) throw new ValidationException(PostErrors.LostPostCannotBeAssociatedWithOrganization);

            location = organization.Location;
            displayAddress = organization.DisplayAddress;
            externalPlaceId = organization.ExternalPlaceId;
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = command.AuthorId,
            OrganizationId = command.OrganizationId,
            PostType = postType,
            Item = command.Item,
            Location = location,
            ExternalPlaceId = externalPlaceId,
            DisplayAddress = displayAddress,
            Embedding = null,
            EmbeddingStatus = EmbeddingStatus.Pending,
            PostMatchingStatus = PostMatchingStatus.Pending,
            ContentHash = hasher.HashStrings(JsonSerializer.Serialize(command.Item)),
            EventTime = command.EventTime,
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
