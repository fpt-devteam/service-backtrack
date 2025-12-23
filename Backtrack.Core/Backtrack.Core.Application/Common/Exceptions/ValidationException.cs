using Backtrack.Core.Application.Common.Exceptions.Errors;

namespace Backtrack.Core.Application.Common.Exceptions
{
    public sealed class ValidationException : DomainException
    {
        public ValidationException(Error error) : base(error) { }
    }
}
