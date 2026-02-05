using Backtrack.Core.Application.Exceptions.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrack.Core.Application.Exceptions
{
    public sealed class NotFoundException : DomainException
    {
        public NotFoundException(Error error) : base(error) { }
    }
}
