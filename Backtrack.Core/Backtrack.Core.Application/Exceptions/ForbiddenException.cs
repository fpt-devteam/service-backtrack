using Backtrack.Core.Application.Exceptions.Errors;

namespace Backtrack.Core.Application.Exceptions
{
    public sealed class ForbiddenException : DomainException
    {
        public ForbiddenException(Error error) : base(error) { }
    }
}
