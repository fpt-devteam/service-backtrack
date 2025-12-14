namespace Backtrack.Core.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public Error Error { get; }

        public ValidationException(Error error) : base(error.Message)
        {
            Error = error;
        }
    }
}
