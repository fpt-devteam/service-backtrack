using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostMatchings;
using Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostContentEmbedding;
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
    IPostImageRepository postImageRepository,
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

        var firstImageUrl = command.Images.Length > 0 ? command.Images[0].Url : string.Empty;

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = command.AuthorId,
            OrganizationId = command.OrganizationId,
            PostType = postType,
            ItemName = command.ItemName,
            Description = command.Description,
            Location = location,
            ExternalPlaceId = externalPlaceId,
            DisplayAddress = displayAddress,
            MultimodalEmbedding = null,
            ContentEmbeddingStatus = ContentEmbeddingStatus.Pending,
            PostMatchingStatus = PostMatchingStatus.Pending,
            ContentHash = hasher.HashStrings(
                command.ItemName,
                command.Description,
                firstImageUrl),
            EventTime = command.EventTime,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await postRepository.CreateAsync(post);

        var images = new List<PostImage>();
        for (int i = 0; i < command.Images.Length; i++)
        {
            var input = command.Images[i];
            // log image info but not the base64 data
            logger.LogInformation(
                "Adding image for Post {PostId}: Url={Url}, MimeType={MimeType}, FileName={FileName}, FileSizeBytes={FileSizeBytes}",
                post.Id, input.Url, input.MimeType, input.FileName, input.FileSizeBytes);
            var image = new PostImage
            {
                Id = Guid.NewGuid(),
                PostId = post.Id,
                Url = input.Url,
                Base64Data = input.Base64Data,
                MimeType = input.MimeType,
                FileName = input.FileName,
                FileSizeBytes = input.FileSizeBytes,
                DisplayOrder = i,
            };
            await postImageRepository.CreateAsync(image);
            images.Add(image);
        }


        await postImageRepository.SaveChangesAsync();

        backgroundJobService.EnqueueJob<PostEmbeddingOrchestrator>(orchestrator => orchestrator.GenerateEmbeddingAndFindMatchesAsync(post.Id));

        return new PostResult
        {
            Id = post.Id,
            Organization = post.Organization?.ToOrganizationOnPost(),
            PostType = post.PostType.ToString(),
            ItemName = post.ItemName,
            Description = post.Description,
            Images = images.Select(i => i.ToPostImageResult()).ToList(),
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        };
    }
}
