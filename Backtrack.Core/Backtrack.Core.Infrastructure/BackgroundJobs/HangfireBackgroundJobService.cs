using Backtrack.Core.Application.Common.Interfaces.BackgroundJobs;
using Hangfire;
using MediatR;

namespace Backtrack.Core.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Hangfire-based implementation of IBackgroundJobService.
    /// Enqueues commands to be executed asynchronously via MediatR.
    /// </summary>
    public class HangfireBackgroundJobService : IBackgroundJobService
    {
        public string EnqueueJob<TCommand>(TCommand command) where TCommand : IRequest
        {
            var jobId = BackgroundJob.Enqueue<MediatorJobExecutor<TCommand>>(
                executor => executor.Execute(command, CancellationToken.None));

            return jobId;
        }
    }
}
