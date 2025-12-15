using MediatR;

namespace Backtrack.Core.Application.Common;

public sealed record PagedQuery
{
    public int Offset { get; init; }
    public int Limit { get; init; }

    public static PagedQuery FromPage(int page, int pageSize)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

        return new PagedQuery
        {
            Offset = (page - 1) * pageSize,
            Limit = pageSize
        };
    }
}