using Backtrack.Core.Application.Common.Exceptions;
using Backtrack.Core.Application.Common.Interfaces.AI;
using Backtrack.Core.Application.Common.Interfaces.Helpers;
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
        Post post = await _postRepository.GetByIdAsync(request.PostId) ?? throw new DomainException(PostErrors.NotFound);

        string newContentHash = _hasher.HashStrings(post.ItemName, post.Description);

        if (post.ContentHash == newContentHash && post.ContentEmbedding is not null)
        {
            return Unit.Value;
        }

        var contentForEmbedding = $"{post.ItemName}\n{post.Description}";
        var newEmbedding = await _embeddingService.GenerateEmbeddingAsync(contentForEmbedding, cancellationToken);

        post.ContentEmbedding = newEmbedding;
        post.ContentHash = newContentHash;

        _postRepository.Update(post);
        await _postRepository.SaveChangesAsync();

        return Unit.Value;
    }
}