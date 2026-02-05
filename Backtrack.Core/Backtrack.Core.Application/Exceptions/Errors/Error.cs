using System.Net;

namespace Backtrack.Core.Application.Exceptions.Errors
{
    public sealed record Error(string Code, string Message);
}