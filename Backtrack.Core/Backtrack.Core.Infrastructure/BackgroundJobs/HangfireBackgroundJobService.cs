using Backtrack.Core.Application.Interfaces.BackgroundJobs;
using Hangfire;
using MediatR;
using System.Linq.Expressions;

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

        public string EnqueueJob(Expression<Action> methodCall) => BackgroundJob.Enqueue(methodCall);

        public string EnqueueJob<T>(Expression<Action<T>> methodCall) => BackgroundJob.Enqueue(methodCall);

        public string EnqueueJob<T>(Expression<Func<T, Task>> methodCall) => BackgroundJob.Enqueue(methodCall);
    }
}
