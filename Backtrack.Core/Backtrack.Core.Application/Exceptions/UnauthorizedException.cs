using Backtrack.Core.Application.Exceptions.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrack.Core.Application.Exceptions
{
    public sealed class UnauthorizedException : DomainException
    {
        public UnauthorizedException(Error error) : base(error) { }
    }
}
