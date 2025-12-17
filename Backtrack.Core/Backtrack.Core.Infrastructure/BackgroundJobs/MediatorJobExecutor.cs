using MediatR;

namespace Backtrack.Core.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Executes MediatR commands within a Hangfire job context.
    /// This enables proper dependency injection scoping for background jobs.
    /// </summary>
    /// <typeparam name="TCommand">The command type implementing IRequest</typeparam>
    public class MediatorJobExecutor<TCommand> where TCommand : IRequest
    {
        private readonly IMediator _mediator;

        public MediatorJobExecutor(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Execute(TCommand command, CancellationToken cancellationToken)
        {
            await _mediator.Send(command, cancellationToken);
        }
    }
}
