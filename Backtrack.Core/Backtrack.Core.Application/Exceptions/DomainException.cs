using Backtrack.Core.Application.Exceptions.Errors;
using System;

namespace Backtrack.Core.Application.Exceptions
{
    public abstract class DomainException : Exception
    {
        public Error Error { get; }
        public IReadOnlyDictionary<string, string[]>? Details { get; }

        protected DomainException(Error error, IReadOnlyDictionary<string, string[]>? details = null) : base(error.Message)
        {
            Error = error;
            Details = details;
        }
    }
}
