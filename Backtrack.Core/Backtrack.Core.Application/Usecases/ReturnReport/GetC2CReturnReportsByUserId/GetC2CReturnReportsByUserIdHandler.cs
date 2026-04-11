using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetC2CReturnReportsByUserId;

public sealed class GetC2CReturnReportsByUserIdHandler(
    IC2CReturnReportRepository returnReportRepository)
    : IRequestHandler<GetC2CReturnReportsByUserIdQuery, PagedResult<C2CReturnReportResult>>
{
    public async Task<PagedResult<C2CReturnReportResult>> Handle(
        GetC2CReturnReportsByUserIdQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await returnReportRepository.GetByUserAsync(
            query.UserId, query.Page, query.PageSize, query.Status, cancellationToken);

        var results = items.Select(r => new C2CReturnReportResult
        {
            Id = r.Id,
            Finder = r.FinderPost?.Author.ToUserResult() ?? r.Finder.ToUserResult(),
            Owner = r.OwnerPost?.Author.ToUserResult() ?? r.Owner?.ToUserResult(),
            FinderPost = r.FinderPost?.ToPostResult(),
            OwnerPost = r.OwnerPost?.ToPostResult(),
            Status = r.Status.ToString(),
            ActivatedByRole = r.ActivatedById == r.FinderId ? "Finder"
                            : r.ActivatedById == r.OwnerId ? "Owner"
                            : null,
            ConfirmedAt = r.ConfirmedAt,
            ExpiresAt = r.ExpiresAt,
            CreatedAt = r.CreatedAt
        }).ToList();

        return new PagedResult<C2CReturnReportResult>(total, results);
    }
}
