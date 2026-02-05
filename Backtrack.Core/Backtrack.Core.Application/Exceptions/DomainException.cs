using Backtrack.Core.Application.Exceptions.Errors;
using System;

namespace Backtrack.Core.Application.Exceptions
{
    public abstract class DomainException : Exception
    {
        public Error Error { get; }

        protected DomainException(Error error) : base(error.Message)
            => Error = error;
    }
}
