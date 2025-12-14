using System.Net;

namespace Backtrack.Core.Application.Common.Exceptions
{
    public sealed record Error(string Code, string Message, HttpStatusCode HttpStatusCode);
}