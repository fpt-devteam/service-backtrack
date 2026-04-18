using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Utils;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostEmbedding;

public sealed class UpdatePostEmbeddingHandler(
    IPostRepository postRepository,
    IHasher hasher,
    IEmbeddingService embeddingService,
    ILogger<UpdatePostEmbeddingHandler> logger) : IRequestHandler<UpdatePostEmbeddingCommand>
{
    public async Task<Unit> Handle(UpdatePostEmbeddingCommand request, CancellationToken cancellationToken)
    {
        var post = await GetPostOrThrowAsync(request.PostId);

        if (IsEmbeddingUpToDate(post))
        {
            logger.LogDebug("Post {PostId} embedding is up-to-date, skipping.", post.Id);
            return Unit.Value;
        }

        await MarkAsProcessingAsync(post);
        await GenerateAndSaveEmbeddingAsync(post, cancellationToken);

        return Unit.Value;
    }

    private async Task<Post> GetPostOrThrowAsync(Guid postId)
    {
        return await postRepository.GetByIdAsync(postId, true)
            ?? throw new NotFoundException(PostErrors.NotFound);
    }

    private static bool IsEmbeddingUpToDate(Post post)
    {
        return post.Embedding is not null && post.EmbeddingStatus == EmbeddingStatus.Ready;
    }

    private async Task MarkAsProcessingAsync(Post post)
    {
        post.EmbeddingStatus = EmbeddingStatus.Processing;
        postRepository.Update(post);
        await postRepository.SaveChangesAsync();
    }

    private async Task GenerateAndSaveEmbeddingAsync(Post post, CancellationToken cancellationToken)
    {
        try
        {
            var content = PostDocumentUtil.BuildDocument(post);
            var embedding = await embeddingService.GenerateDocumentEmbeddingAsync(content, cancellationToken);

            post.Embedding = embedding;
            post.EmbeddingStatus = EmbeddingStatus.Ready;

            var hash = hasher.HashStrings(content);
            if (post.PersonalBelongingDetail is not null) post.PersonalBelongingDetail.ContentHash = hash;
            else if (post.ElectronicDetail is not null) post.ElectronicDetail.ContentHash = hash;
            else if (post.OtherDetail is not null) post.OtherDetail.ContentHash = hash;

            postRepository.Update(post);
            await postRepository.SaveChangesAsync();

            logger.LogInformation("Embedding saved successfully for Post {PostId}.", post.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate embedding for Post {PostId}. Marking as Failed.", post.Id);

            post.EmbeddingStatus = EmbeddingStatus.Failed;
            postRepository.Update(post);
            await postRepository.SaveChangesAsync();

            throw;
        }
    }
}
