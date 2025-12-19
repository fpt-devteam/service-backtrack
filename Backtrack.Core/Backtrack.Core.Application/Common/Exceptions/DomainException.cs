using Backtrack.Core.Application.Common.Exceptions.Errors;
using System;

namespace Backtrack.Core.Application.Common.Exceptions
{
    public abstract class DomainException : Exception
    {
        public Error Error { get; }

        protected DomainException(Error error) : base(error.Message)
            => Error = error;
    }
}
