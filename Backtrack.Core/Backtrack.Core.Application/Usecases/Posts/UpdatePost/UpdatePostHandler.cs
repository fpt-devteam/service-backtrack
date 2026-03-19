using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Application.Usecases.PostImages;
using Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostContentEmbedding;
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

    public UpdatePostHandler(
        IPostRepository postRepository,
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository,
        IBackgroundJobService backgroundJobService)
    {
        _postRepository = postRepository;
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<PostResult> Handle(UpdatePostCommand command, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(command.PostId, true);
        if (post == null) throw new NotFoundException(PostErrors.NotFound);
        if (post.AuthorId != command.AuthorId) throw new ForbiddenException(PostErrors.Forbidden);
        bool needsReEmbedding = false;

        if (command.PostType != null)
        {
            if (!Enum.TryParse<PostType>(command.PostType, ignoreCase: true, out var postType)) throw new ValidationException(PostErrors.InvalidPostType);
            if (post.PostType != postType)
            {
                post.PostType = postType;
                needsReEmbedding = true;
            }
        }

        if (command.OrganizationId.HasValue && post.OrganizationId != command.OrganizationId)
        {
            var organization = await _organizationRepository.GetByIdAsync(command.OrganizationId.Value)
                ?? throw new NotFoundException(OrganizationErrors.NotFound);

            if (await _membershipRepository.GetByOrgAndUserAsync(organization.Id, command.AuthorId) == null)
            {
                throw new ValidationException(MembershipErrors.MemberNotFound);
            }

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

        if (command.ItemName != null && post.ItemName != command.ItemName)
        {
            post.ItemName = command.ItemName;
            needsReEmbedding = true;
        }

        if (command.Description != null && post.Description != command.Description)
        {
            post.Description = command.Description;
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
        post.EventTime = command.EventTime.HasValue ? command.EventTime.Value : post.EventTime;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        if (needsReEmbedding)
        {
            post.ContentEmbeddingStatus = ContentEmbeddingStatus.Pending;
            post.PostMatchingStatus = PostMatchingStatus.Pending;
        }
        await _postRepository.SaveChangesAsync();

        if (needsReEmbedding) _backgroundJobService.EnqueueJob<PostEmbeddingOrchestrator>(orchestrator => orchestrator.GenerateEmbeddingAndFindMatchesAsync(post.Id));

        return new PostResult
        {
            Id = post.Id,
            Author = post.Author?.ToAuthorResult(),
            Organization = post.Organization?.ToOrganizationOnPost(),
            PostType = post.PostType.ToString(),
            ItemName = post.ItemName,
            Description = post.Description,
            Images = post.Images.Select(i => i.ToPostImageResult()).ToList(),
            Location = post.Location,
            ExternalPlaceId = post.ExternalPlaceId,
            DisplayAddress = post.DisplayAddress,
            EventTime = post.EventTime,
            CreatedAt = post.CreatedAt
        };
    }
}
