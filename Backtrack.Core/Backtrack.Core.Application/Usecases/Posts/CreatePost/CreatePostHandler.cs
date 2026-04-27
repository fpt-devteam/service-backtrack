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
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.Posts.CreatePost;

public sealed class CreatePostHandler(
    IPostRepository postRepository,
    ISubcategoryRepository subcategoryRepository,
    IHasher hasher,
    IBackgroundJobService backgroundJobService,
    ILogger<CreatePostHandler> logger) : IRequestHandler<CreatePostCommand, PostResult>
{
    public async Task<PostResult> Handle(CreatePostCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ItemCategory>(command.Category, ignoreCase: true, out var category))
            throw new ValidationException(PostErrors.InvalidCategory);

        if (!Enum.TryParse<PostType>(command.PostType, ignoreCase: true, out var postType))
            throw new ValidationException(PostErrors.InvalidPostType);

        var subcategory = await subcategoryRepository.GetByCodeAsync(command.SubcategoryCode, cancellationToken)
            ?? throw new NotFoundException(PostErrors.SubcategoryNotFound);

        var post = new Post
        {
            Id                 = Guid.NewGuid(),
            AuthorId           = command.AuthorId,
            OrganizationId     = null,
            PostType           = postType,
            Status             = PostStatus.Active,
            Category           = category,
            SubcategoryId      = subcategory.Id,
            Location           = command.Location!,
            ExternalPlaceId    = command.ExternalPlaceId,
            DisplayAddress     = command.DisplayAddress!,
            Embedding          = null,
            EmbeddingStatus    = EmbeddingStatus.Pending,
            PostMatchingStatus = PostMatchingStatus.Pending,
            EventTime          = command.EventTime ?? DateTimeOffset.UtcNow,
            ImageUrls          = command.ImageUrls.ToList(),
            PostTitle          = command.PostTitle,
            CreatedAt          = DateTimeOffset.UtcNow
        };

        AttachDetail(post, command, hasher);
        SetDetailContentHash(post, hasher);

        await postRepository.CreateAsync(post);
        await postRepository.SaveChangesAsync();

        backgroundJobService.EnqueueJob<PostEmbeddingOrchestrator>(
            orchestrator => orchestrator.GenerateEmbeddingAndFindMatchesAsync(post.Id));

        return post.ToPostResult();
    }

    private static void SetDetailContentHash(Post post, IHasher hasher)
    {
        var hash = hasher.HashStrings(PostDocumentUtil.BuildDocument(post));

        _ = post.Category switch
        {
            ItemCategory.PersonalBelongings when post.PersonalBelongingDetail is { } d => d.ContentHash = hash,
            ItemCategory.Cards              when post.CardDetail is { } d              => d.ContentHash = hash,
            ItemCategory.Electronics        when post.ElectronicDetail is { } d        => d.ContentHash = hash,
            ItemCategory.Others             when post.OtherDetail is { } d             => d.ContentHash = hash,
            _ => null,
        };
    }

    private void AttachDetail(Post post, CreatePostCommand command, IHasher hasher)
    {
        var attached = post.Category switch
        {
            ItemCategory.PersonalBelongings when command.PersonalBelongingDetail is { } pb
                => (Action)(() => post.PersonalBelongingDetail = pb.ToEntity(post.Id)),
            ItemCategory.Cards when command.CardDetail is { } cd
                => () => post.CardDetail = cd.ToEntity(post.Id, hasher),
            ItemCategory.Electronics when command.ElectronicDetail is { } ed
                => () => post.ElectronicDetail = ed.ToEntity(post.Id),
            ItemCategory.Others when command.OtherDetail is { } od
                => () => post.OtherDetail = od.ToEntity(post.Id),
            _ => null,
        };

        if (attached is not null)
            attached();
        else
            logger.LogWarning(
                "No detail attached for post {PostId}. Category={Category}, " +
                "PersonalBelonging={PB}, Card={Card}, Electronic={Electronic}, Other={Other}",
                post.Id, post.Category,
                command.PersonalBelongingDetail is not null,
                command.CardDetail is not null,
                command.ElectronicDetail is not null,
                command.OtherDetail is not null);
    }

}
