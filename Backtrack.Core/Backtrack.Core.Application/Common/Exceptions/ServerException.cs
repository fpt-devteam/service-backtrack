using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrack.Core.Application.Common.Exceptions
{
    public class ServerException : Exception
    {
        public Error Error { get; }

        public ServerException(Error error) : base(error.Message)
        {
            Error = error;
        }
    }
}
