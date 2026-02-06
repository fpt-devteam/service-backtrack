using Backtrack.Core.Application.Exceptions.Errors;

namespace Backtrack.Core.Application.Exceptions
{
    public sealed class ValidationException : DomainException
    {
        public ValidationException(Error error) : base(error) { }
    }
}
