using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

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

    // -------------------------------------------------------------------------
    // Steps
    // -------------------------------------------------------------------------

    private async Task<Post> GetPostOrThrowAsync(Guid postId)
    {
        return await postRepository.GetByIdAsync(postId, true)
            ?? throw new NotFoundException(PostErrors.NotFound);
    }

    private bool IsEmbeddingUpToDate(Post post)
    {
        var currentHash = hasher.HashStrings(JsonSerializer.Serialize(post.Item));

        return post.ContentHash == currentHash
            && post.Embedding is not null
            && post.EmbeddingStatus == EmbeddingStatus.Ready;
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
            var content = BuildContent(post);
            var embedding = await embeddingService.GenerateDocumentEmbeddingAsync(content, cancellationToken);
            var newContentHash = hasher.HashStrings(JsonSerializer.Serialize(post.Item));

            post.Embedding = embedding;
            post.ContentHash = newContentHash;
            post.EmbeddingStatus = EmbeddingStatus.Ready;

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

            throw; // Re-throw so Hangfire can retry
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string BuildContent(Post post)
    {
        var item = post.Item;
        var sb = new StringBuilder();

        sb.Append($"A {item.Category.ToString()} item called {item.ItemName}");

        if (!string.IsNullOrWhiteSpace(item.Brand))
            sb.Append($" made by {item.Brand}");

        sb.Append('.');

        if (!string.IsNullOrWhiteSpace(item.Color))
            sb.Append($" It is {item.Color} in color.");

        if (!string.IsNullOrWhiteSpace(item.Material))
            sb.Append($" Made of {item.Material}.");

        if (!string.IsNullOrWhiteSpace(item.Size))
            sb.Append($" Size is {item.Size}.");

        if (!string.IsNullOrWhiteSpace(item.Condition))
            sb.Append($" Condition: {item.Condition}.");

        if (!string.IsNullOrWhiteSpace(item.DistinctiveMarks))
            sb.Append($" It has distinctive marks: {item.DistinctiveMarks}.");

        if (!string.IsNullOrWhiteSpace(item.AdditionalDetails))
            sb.Append($" {item.AdditionalDetails}");

        return sb.ToString().Trim();
    }
}
