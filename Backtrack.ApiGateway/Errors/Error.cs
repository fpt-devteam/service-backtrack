using System.Net;

namespace Backtrack.ApiGateway.Errors
{
    public sealed record Error(string Code, string Message, HttpStatusCode HttpStatusCode);
}
