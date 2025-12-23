using Backtrack.Core.Application.Common.Exceptions.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrack.Core.Application.Common.Exceptions
{
    public sealed class ConflictException : DomainException
    {
        public ConflictException(Error error) : base(error) { }
    }
}
