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

namespace Backtrack.Core.Application.Usecases.Posts.CreatePost;

public sealed class CreatePostHandler(
    IPostRepository postRepository,
    ISubcategoryRepository subcategoryRepository,
    IHasher hasher,
    IBackgroundJobService backgroundJobService) : IRequestHandler<CreatePostCommand, PostResult>
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
        if (post.Category == ItemCategory.Cards) return;

        var hash = hasher.HashStrings(PostDocumentUtil.BuildDocument(post));

        if (post.PersonalBelongingDetail is not null) post.PersonalBelongingDetail.ContentHash = hash;
        else if (post.ElectronicDetail is not null) post.ElectronicDetail.ContentHash = hash;
        else if (post.OtherDetail is not null) post.OtherDetail.ContentHash = hash;
    }

    private static void AttachDetail(Post post, CreatePostCommand command, IHasher hasher)
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

}
