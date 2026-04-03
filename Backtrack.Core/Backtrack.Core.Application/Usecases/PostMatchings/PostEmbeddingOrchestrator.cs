using Backtrack.Core.Application.Usecases.PostMatchings;
using Backtrack.Core.Application.Usecases.PostMatchings.FindAndSavePostMatches;
using Backtrack.Core.Application.Usecases.PostMatchings.UpdatePostEmbedding;
using MediatR;

namespace Backtrack.Core.Application.Usecases.PostMatchings;

public class PostEmbeddingOrchestrator
{
    private readonly IMediator _mediator;

    public PostEmbeddingOrchestrator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task GenerateEmbeddingAndFindMatchesAsync(Guid postId)
    {
        await _mediator.Send(new UpdatePostEmbeddingCommand(postId));
        // await _mediator.Send(new FindAndSavePostMatchesCommand(postId));
    }
}