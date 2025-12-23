using System.Net;

namespace Backtrack.Core.Application.Common.Exceptions.Errors
{
    public sealed record Error(string Code, string Message);
}