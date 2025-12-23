using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Common.Exceptions.Errors;
using Backtrack.Core.Application.Common.Interfaces.AI;
using Backtrack.Core.Application.Common.Interfaces.Helpers;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using MediatR;

namespace Backtrack.Core.Application.Posts.Commands.UpdatePostContentEmbedding;

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
        Post post = await _postRepository.GetByIdAsync(request.PostId) ?? throw new NotFoundException(PostErrors.NotFound);

        string newContentHash = _hasher.HashStrings(post.ItemName, post.Description);

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
            // Format: Post type + item name (repeated for emphasis) + description
            var contentForEmbedding = $@"Post Type: {post.PostType}
Item: {post.ItemName}
Description: {post.Description}

This is a {post.PostType.ToString().ToLower()} post about {post.ItemName.ToLower()}.";

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