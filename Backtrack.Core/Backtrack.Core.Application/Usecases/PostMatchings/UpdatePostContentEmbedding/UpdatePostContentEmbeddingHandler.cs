using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostContentEmbedding;

public sealed class UpdatePostContentEmbeddingHandler(
    IPostRepository postRepository,
    IHasher hasher,
    IEmbeddingService embeddingService,
    IImageFetcher imageFetcher,
    ILogger<UpdatePostContentEmbeddingHandler> logger) : IRequestHandler<UpdatePostContentEmbeddingCommand>
{
    public async Task<Unit> Handle(UpdatePostContentEmbeddingCommand request, CancellationToken cancellationToken)
    {
        var post = await GetPostOrThrowAsync(request.PostId);

        if (IsEmbeddingUpToDate(post))
        {
            logger.LogDebug("Post {PostId} embeddings are up-to-date, skipping.", post.Id);
            return Unit.Value;
        }

        await MarkAsProcessingAsync(post);
        await GenerateAndSaveEmbeddingsAsync(post, cancellationToken);

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
        var firstImageUrl = GetFirstImageUrl(post);

        var currentHash = hasher.HashStrings(
            post.ItemName,
            post.Description,
            firstImageUrl ?? string.Empty);

        var allEmbeddingsPresent =
            post.MultimodalEmbedding is not null &&
            post.TextEmbedding is not null &&
            post.ImageEmbedding is not null &&
            post.ContentEmbeddingStatus == ContentEmbeddingStatus.Ready;

        return post.ContentHash == currentHash && allEmbeddingsPresent;
    }

    private async Task MarkAsProcessingAsync(Post post)
    {
        post.ContentEmbeddingStatus = ContentEmbeddingStatus.Processing;
        postRepository.Update(post);
        await postRepository.SaveChangesAsync();
    }

    private async Task GenerateAndSaveEmbeddingsAsync(Post post, CancellationToken cancellationToken)
    {
        try
        {
            var firstImageUrl = GetFirstImageUrl(post);
            var textContent = BuildTextContent(post);
            var image = await FetchImageAsync(firstImageUrl, post.Id, cancellationToken);

            logger.LogInformation(
                "Generating embeddings for Post {PostId}. HasImage: {HasImage}",
                post.Id, image is not null);

            var (textEmbedding, imageEmbedding, multimodalEmbedding) =
                await GenerateAllEmbeddingsAsync(textContent, image, post.Id, cancellationToken);

            var newContentHash = hasher.HashStrings(
                post.ItemName,
                post.Description,
                firstImageUrl ?? string.Empty);

            post.TextEmbedding = textEmbedding;
            post.ImageEmbedding = imageEmbedding;
            post.MultimodalEmbedding = multimodalEmbedding;
            post.ContentHash = newContentHash;
            post.ContentEmbeddingStatus = ContentEmbeddingStatus.Ready;

            postRepository.Update(post);
            await postRepository.SaveChangesAsync();

            logger.LogInformation("Embeddings saved successfully for Post {PostId}.", post.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate embeddings for Post {PostId}. Marking as Failed.", post.Id);

            post.ContentEmbeddingStatus = ContentEmbeddingStatus.Failed;
            postRepository.Update(post);
            await postRepository.SaveChangesAsync();

            throw; // Re-throw so Hangfire can retry
        }
    }

    // -------------------------------------------------------------------------
    // Embedding generation
    // -------------------------------------------------------------------------

    private async Task<(float[]? text, float[]? image, float[]? multimodal)> GenerateAllEmbeddingsAsync(
        string textContent,
        FetchedImage? image,
        Guid postId,
        CancellationToken cancellationToken)
    {
        var textTask = GenerateTextEmbeddingAsync(textContent, postId, cancellationToken);
        var imageTask = GenerateImageEmbeddingAsync(image, postId, cancellationToken);
        var multimodalTask = GenerateMultimodalEmbeddingAsync(textContent, image, postId, cancellationToken);

        await Task.WhenAll(textTask, imageTask, multimodalTask);

        return (await textTask, await imageTask, await multimodalTask);
    }

    private async Task<float[]?> GenerateTextEmbeddingAsync(
        string textContent, Guid postId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("Generating text embedding for Post {PostId}.", postId);
            return await embeddingService.GenerateTextEmbeddingAsync(textContent, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Text embedding failed for Post {PostId}.", postId);
            throw;
        }
    }

    private async Task<float[]?> GenerateImageEmbeddingAsync(
        FetchedImage? image, Guid postId, CancellationToken cancellationToken)
    {
        if (image is null)
        {
            logger.LogDebug("No image available for Post {PostId}, skipping image embedding.", postId);
            return null;
        }

        try
        {
            logger.LogDebug("Generating image embedding for Post {PostId}.", postId);
            return await embeddingService.GenerateMultimodalEmbeddingAsync(
                text: null, image.Base64, image.MimeType, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Image embedding failed for Post {PostId}.", postId);
            throw;
        }
    }

    private async Task<float[]?> GenerateMultimodalEmbeddingAsync(
        string textContent, FetchedImage? image, Guid postId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("Generating multimodal embedding for Post {PostId}.", postId);
            return await embeddingService.GenerateMultimodalEmbeddingAsync(
                textContent, image?.Base64, image?.MimeType, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Multimodal embedding failed for Post {PostId}.", postId);
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<FetchedImage?> FetchImageAsync(
        string? imageUrl, Guid postId, CancellationToken cancellationToken)
    {
        if (imageUrl is null) return null;

        try
        {
            var image = await imageFetcher.FetchAsync(imageUrl, cancellationToken);

            if (image is null)
                logger.LogWarning("Image fetch returned null for Post {PostId}. URL: {Url}", postId, imageUrl);

            return image;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch image for Post {PostId}. URL: {Url}", postId, imageUrl);
            throw;
        }
    }

    private static string? GetFirstImageUrl(Post post)
        => post.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.Url;

    private static string BuildTextContent(Post post)
        => $"Item: {post.ItemName}\nDescription: {post.Description}\n\nThis item is {post.ItemName.ToLower()}";
}