using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Posts;
using Backtrack.Core.Application.Usecases.Users;
using MediatR;

namespace Backtrack.Core.Application.Usecases.ReturnReport.GetC2CReturnReportsByPartnerId;

public sealed class GetC2CReturnReportsByPartnerIdHandler(
    IC2CReturnReportRepository returnReportRepository)
    : IRequestHandler<GetC2CReturnReportsByPartnerIdQuery, PagedResult<C2CReturnReportResult>>
{
    public async Task<PagedResult<C2CReturnReportResult>> Handle(
        GetC2CReturnReportsByPartnerIdQuery query, CancellationToken cancellationToken)
    {
        var items = await returnReportRepository.GetByPartnerAsync(
            query.UserId, query.PartnerId, cancellationToken);

        var results = items.Select(r => new C2CReturnReportResult
        {
            Id = r.Id,
            Finder = r.Finder!.ToUserResult(),
            Owner = r.Owner?.ToUserResult(),
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

        return new PagedResult<C2CReturnReportResult>(results.Count, results);
    }
}
