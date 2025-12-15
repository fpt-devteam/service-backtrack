using Backtrack.Core.Application.Common.Exceptions;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Backtrack.Core.Application.Common;

public sealed record PagedQuery
{
    public int Offset { get; init; }
    public int Limit { get; init; }

    public static PagedQuery FromPage(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1) throw new Exceptions.ValidationException(PaginationErrors.InvalidPagedQuery);

        return new PagedQuery
        {
            Offset = (page - 1) * pageSize,
            Limit = pageSize
        };
    }
}