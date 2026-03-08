using MediatR;
using System.Linq.Expressions;

namespace Backtrack.Core.Application.Interfaces.BackgroundJobs
{
    /// <summary>
    /// Interface for enqueuing background jobs.
    /// Used to decouple the application layer from specific background job implementations.
    /// </summary>
    public interface IBackgroundJobService
    {
        /// <summary>
        /// Enqueues a job to be executed in the background.
        /// </summary>
        /// <typeparam name="TCommand">The command type to execute</typeparam>
        /// <param name="command">The command instance</param>
        /// <returns>Job identifier</returns>
        string EnqueueJob<TCommand>(TCommand command) where TCommand : IRequest;
        string EnqueueJob(Expression<Action> methodCall);
        string EnqueueJob<T>(Expression<Action<T>> methodCall);
        string EnqueueJob<T>(Expression<Func<T, Task>> methodCall);

    }
}
