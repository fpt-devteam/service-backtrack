using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Backtrack.Core.Application.Common.Exceptions
{
    public static class PaginationErrors
    {
        public static readonly Error InvalidPagedQuery = new(
            Code: "InvalidPagedQuery",
            Message: "Limit and offset must be greater than 0",
            HttpStatusCode.BadRequest);
    }
}
