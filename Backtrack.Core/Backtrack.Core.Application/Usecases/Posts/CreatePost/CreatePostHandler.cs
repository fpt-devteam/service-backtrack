using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostMatchings;
using Backtrack.Core.Application.Usecases.Posts.UpdatePostContentEmbedding;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.Posts.CreatePost;

public sealed class CreatePostHandler : IRequestHandler<CreatePostCommand, PostResult>
{
    private readonly IPostRepository _postRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IHasher _hasher;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<CreatePostHandler> _logger;

    public CreatePostHandler(
        IPostRepository postRepository,
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository,
        IHasher hasher,
        IBackgroundJobService backgroundJobService,
        ILogger<CreatePostHandler> logger)
    {
        _postRepository = postRepository;
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
        _hasher = hasher;
        _backgroundJobService = backgroundJobService;
        _logger = logger;
    }

    public async Task<PostResult> Handle(CreatePostCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PostType>(command.PostType, ignoreCase: true, out var postType))
        {
            throw new ValidationException(PostErrors.InvalidPostType);
        }

        GeoPoint? location = command.Location;
        string? displayAddress = command.DisplayAddress;
        string? externalPlaceId = command.ExternalPlaceId;

        if (command.OrganizationId.HasValue)
        {
            var organization = await _organizationRepository.GetByIdAsync(command.OrganizationId.Value)
                ?? throw new NotFoundException(OrganizationErrors.NotFound);

            if (await _membershipRepository.GetByOrgAndUserAsync(organization.Id, command.AuthorId) == null)
            {
                throw new ValidationException(MembershipErrors.MemberNotFound);
            }

            if (postType == PostType.Lost) throw new ValidationException(PostErrors.LostPostCannotBeAssociatedWithOrganization);

            location = organization.Location;
            displayAddress = organization.DisplayAddress;
            externalPlaceId = organization.ExternalPlaceId;
        }

        if (location == null || displayAddress == null)
        {
            throw new ValidationException(new Error("LocationRequired", "Location and DisplayAddress are required."));
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = command.AuthorId,
            OrganizationId = command.OrganizationId,
            PostType = postType,
            ItemName = command.ItemName,
            Description = command.Description,
            DistinctiveMarks = command.DistinctiveMarks,
            ImageUrls = command.ImageUrls,
            Location = location,
            ExternalPlaceId = externalPlaceId,
            DisplayAddress = displayAddress,
            MultimodalEmbedding = null, // Will be generated asynchronously
            ContentEmbeddingStatus = ContentEmbeddingStatus.Pending,
            PostMatchingStatus = PostMatchingStatus.Pending,
            ContentHash = _hasher.HashStrings(
                command.ItemName,
                command.Description,
                command.DistinctiveMarks ?? string.Empty,
                command.ImageUrls.Length > 0 ? command.ImageUrls[0] : string.Empty),
            EventTime = command.EventTime,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _postRepository.CreateAsync(post);
        await _postRepository.SaveChangesAsync();

        _backgroundJobService.EnqueueJob<PostEmbeddingOrchestrator>(orchestrator => orchestrator.GenerateEmbeddingAndFindMatchesAsync(post.Id));

        return new PostResult
        {
            Id = post.Id,
            Organization = post.Organization?.ToOrganizationOnPost(),
            PostType = post.PostType.ToString(),
            ItemName = post.ItemName,
            Description = post.Description,
            ImageUrls = post.ImageUrls,
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        };
    }
}
