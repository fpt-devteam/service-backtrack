using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostContentEmbedding;

public sealed class UpdatePostContentEmbeddingHandler(
    IPostRepository postRepository,
    IHasher hasher,
    IEmbeddingService embeddingService,
    IImageFetcher imageFetcher) : IRequestHandler<UpdatePostContentEmbeddingCommand>
{
    public async Task<Unit> Handle(UpdatePostContentEmbeddingCommand request, CancellationToken cancellationToken)
    {
        Post post = await postRepository.GetByIdAsync(request.PostId, true) ?? throw new NotFoundException(PostErrors.NotFound);

        string? firstImageUrl = post.ImageUrls.Length > 0 ? post.ImageUrls[0] : null;

        string newContentHash = hasher.HashStrings(
            post.ItemName,
            post.Description,
            post.DistinctiveMarks ?? string.Empty,
            firstImageUrl ?? string.Empty);

        if (post.ContentHash == newContentHash && post.MultimodalEmbedding is not null && post.ContentEmbeddingStatus == ContentEmbeddingStatus.Ready)
        {
            return Unit.Value;
        }

        post.ContentEmbeddingStatus = ContentEmbeddingStatus.Processing;
        postRepository.Update(post);
        await postRepository.SaveChangesAsync();

        try
        {
            var contentForEmbedding = $"Item: {post.ItemName}\nDescription: {post.Description}";

            if (!string.IsNullOrWhiteSpace(post.DistinctiveMarks))
            {
                contentForEmbedding += $"\nDistinctive marks: {post.DistinctiveMarks}";
            }

            contentForEmbedding += $"\n\nThis item is {post.ItemName.ToLower()}.";

            string? imageBase64 = null;
            string? mimeType = null;

            if (firstImageUrl is not null)
            {
                var image = await imageFetcher.FetchAsync(firstImageUrl, cancellationToken);
                if (image is not null)
                {
                    imageBase64 = image.Base64;
                    mimeType = image.MimeType;
                }
            }

            var newEmbedding = await embeddingService.GenerateMultimodalEmbeddingAsync(contentForEmbedding, imageBase64, mimeType, cancellationToken);

            post.MultimodalEmbedding = newEmbedding;
            post.ContentHash = newContentHash;
            post.ContentEmbeddingStatus = ContentEmbeddingStatus.Ready;

            postRepository.Update(post);
            await postRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            post.ContentEmbeddingStatus = ContentEmbeddingStatus.Failed;
            postRepository.Update(post);
            await postRepository.SaveChangesAsync();

            throw; // Re-throw to allow Hangfire to retry
        }

        return Unit.Value;
    }
}
