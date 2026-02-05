using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.Commands.UpdatePostContentEmbedding;

public sealed class UpdatePostContentEmbeddingHandler : IRequestHandler<UpdatePostContentEmbeddingCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IHasher _hasher;
    private readonly IEmbeddingService _embeddingService;

    public UpdatePostContentEmbeddingHandler(
        IPostRepository postRepository,
        IHasher hasher,
        IEmbeddingService embeddingService)
    {
        _postRepository = postRepository;
        _hasher = hasher;
        _embeddingService = embeddingService;
    }

    public async Task<Unit> Handle(UpdatePostContentEmbeddingCommand request, CancellationToken cancellationToken)
    {
        Post post = await _postRepository.GetByIdAsync(request.PostId, true) ?? throw new NotFoundException(PostErrors.NotFound);

        // Include DistinctiveMarks in hash calculation
        string newContentHash = post.DistinctiveMarks != null
            ? _hasher.HashStrings(post.ItemName, post.Description, post.DistinctiveMarks)
            : _hasher.HashStrings(post.ItemName, post.Description);

        // Skip if content hasn't changed and embedding already exists with Ready status
        if (post.ContentHash == newContentHash && post.ContentEmbedding is not null && post.ContentEmbeddingStatus == ContentEmbeddingStatus.Ready)
        {
            return Unit.Value;
        }

        // Mark as processing
        post.ContentEmbeddingStatus = ContentEmbeddingStatus.Processing;
        _postRepository.Update(post);
        await _postRepository.SaveChangesAsync();

        try
        {
            // Generate rich embedding with structured context
            // NOTE: PostType is excluded to improve cross-type matching (Lost/Found)
            // Format: Item name + description + distinctive marks
            var contentForEmbedding = $@"Item: {post.ItemName}
Description: {post.Description}";

            // Add distinctive marks if present
            if (!string.IsNullOrWhiteSpace(post.DistinctiveMarks))
            {
                contentForEmbedding += $@"
Distinctive marks: {post.DistinctiveMarks}";
            }

            // Add context for better embeddings
            contentForEmbedding += $@"

This item is {post.ItemName.ToLower()}.";

            var newEmbedding = await _embeddingService.GenerateEmbeddingAsync(contentForEmbedding, cancellationToken);

            // Update post with new embedding and mark as ready
            post.ContentEmbedding = newEmbedding;
            post.ContentHash = newContentHash;
            post.ContentEmbeddingStatus = ContentEmbeddingStatus.Ready;

            _postRepository.Update(post);
            await _postRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Mark as failed if embedding generation fails
            post.ContentEmbeddingStatus = ContentEmbeddingStatus.Failed;
            _postRepository.Update(post);
            await _postRepository.SaveChangesAsync();

            throw; // Re-throw to allow Hangfire to retry
        }

        return Unit.Value;
    }
}